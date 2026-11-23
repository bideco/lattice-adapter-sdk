using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Lattice.AdapterSdk
{
    public sealed class LatticeAdapter : IDisposable
    {
        private const int ProtocolVersion = 2;
        private readonly AdapterDescriptor descriptor;
        private readonly Action<string> log;
        private readonly Queue<LatticeEvent> pending = new Queue<LatticeEvent>();
        private readonly object queueLock = new object();
        private readonly object writeLock = new object();
        private readonly ManualResetEventSlim shutdown = new ManualResetEventSlim(false);
        private Thread worker;
        private Thread heartbeatWorker;
        private NamedPipeClientStream readPipe;
        private NamedPipeClientStream writePipe;

        public LatticeAdapter(AdapterDescriptor descriptor, Action<string> log = null)
        {
            this.descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            this.log = log ?? (_ => { });
        }

        public bool Connected { get; private set; }
        public AdapterDescriptor Descriptor => descriptor;

        public event Action<LatticeEvent> RawObserved;
        public event Action<int, int> MouseMoved;
        public event Action<int, bool> MouseButtonChanged;
        public event Action<int, int> MouseScrolled;
        public event Action<int, int> KeyDown;
        public event Action<int, int> KeyUp;
        public event Action<int> CharTyped;
        public event Action SlotActivated;
        public event Action SlotDeactivated;
        public event Action SlotBusy;

        public void Start()
        {
            if (worker != null)
            {
                return;
            }
            worker = new Thread(Run) { IsBackground = true, Name = "Lattice Adapter Client" };
            worker.Start();
        }

        public void PumpMainThread()
        {
            while (true)
            {
                LatticeEvent e;
                lock (queueLock)
                {
                    if (pending.Count == 0)
                    {
                        return;
                    }
                    e = pending.Dequeue();
                }
                Dispatch(e);
            }
        }

        public void Send(LatticeEvent latticeEvent)
        {
            lock (writeLock)
            {
                if (writePipe == null || !writePipe.IsConnected)
                {
                    return;
                }
                var data = latticeEvent.Encode();
                writePipe.Write(data, 0, data.Length);
                writePipe.Flush();
            }
        }

        private void Run()
        {
            while (!shutdown.IsSet)
            {
                try
                {
                    Connect();
                    Send(LatticeEvent.Handshake(ProtocolVersion, descriptor.Slot, descriptor.EncodedGameVersion));
                    Send(LatticeEvent.AdapterInfo(descriptor.CapabilityMask()));
                    StartHeartbeatThread();
                    PumpPipe();
                }
                catch (Exception ex)
                {
                    if (!shutdown.IsSet)
                    {
                        log("Lattice adapter disconnected: " + ex.Message);
                    }
                }
                finally
                {
                    Connected = false;
                    ClosePipes();
                    heartbeatWorker = null;
                }
                shutdown.Wait(1000);
            }
        }

        private void Connect()
        {
            var readName = "latticework-" + descriptor.Slot + "-out";
            var writeName = "latticework-" + descriptor.Slot + "-in";
            readPipe = new NamedPipeClientStream(".", readName, PipeDirection.InOut, PipeOptions.None);
            writePipe = new NamedPipeClientStream(".", writeName, PipeDirection.InOut, PipeOptions.None);
            readPipe.Connect(2000);
            writePipe.Connect(2000);
            if (readPipe.CanTimeout)
            {
                readPipe.ReadTimeout = 250;
            }
            Connected = true;
            log("Connected to Lattice Weaver as P" + descriptor.Slot);
        }

        private void PumpPipe()
        {
            var buffer = new byte[LatticeEvent.Size];
            while (!shutdown.IsSet && readPipe != null)
            {
                var status = ReadExactly(readPipe, buffer);
                if (status == ReadStatus.Timeout)
                {
                    continue;
                }
                if (status == ReadStatus.Disconnected)
                {
                    return;
                }
                LatticeEvent e;
                if (LatticeEvent.TryDecode(buffer, out e))
                {
                    lock (queueLock)
                    {
                        pending.Enqueue(e);
                    }
                }
            }
        }

        private enum ReadStatus
        {
            Complete,
            Timeout,
            Disconnected,
        }

        private static ReadStatus ReadExactly(Stream stream, byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                int read;
                try
                {
                    read = stream.Read(buffer, offset, buffer.Length - offset);
                }
                catch (IOException ex) when (IsReadTimeout(ex))
                {
                    return offset == 0 ? ReadStatus.Timeout : ReadStatus.Disconnected;
                }
                if (read <= 0)
                {
                    return ReadStatus.Disconnected;
                }
                offset += read;
            }
            return ReadStatus.Complete;
        }

        private static bool IsReadTimeout(IOException ex)
        {
            var message = ex.Message ?? string.Empty;
            return message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("semaphore timeout", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void StartHeartbeatThread()
        {
            heartbeatWorker = new Thread(() =>
            {
                while (!shutdown.IsSet)
                {
                    try
                    {
                        Send(LatticeEvent.Heartbeat());
                    }
                    catch
                    {
                        return;
                    }
                    shutdown.Wait(5000);
                }
            })
            {
                IsBackground = true,
                Name = "Lattice Adapter Heartbeat"
            };
            heartbeatWorker.Start();
        }

        private void Dispatch(LatticeEvent e)
        {
            RawObserved?.Invoke(e);
            switch (e.Type)
            {
                case LatticeEventType.MouseMove: MouseMoved?.Invoke(e.Param1, e.Param2); break;
                case LatticeEventType.MouseButton: MouseButtonChanged?.Invoke(e.Param1, e.Param2 != 0); break;
                case LatticeEventType.MouseScroll: MouseScrolled?.Invoke(e.Param1, e.Param2); break;
                case LatticeEventType.KeyPress: KeyDown?.Invoke(e.Param1, e.Param2); break;
                case LatticeEventType.KeyRelease: KeyUp?.Invoke(e.Param1, e.Param2); break;
                case LatticeEventType.Char: CharTyped?.Invoke(e.Param1); break;
                case LatticeEventType.SlotActive: SlotActivated?.Invoke(); break;
                case LatticeEventType.SlotInactive: SlotDeactivated?.Invoke(); break;
                case LatticeEventType.SlotBusy: SlotBusy?.Invoke(); break;
            }
        }

        private void ClosePipes()
        {
            lock (writeLock)
            {
                readPipe?.Dispose();
                writePipe?.Dispose();
                readPipe = null;
                writePipe = null;
            }
        }

        public void Dispose()
        {
            shutdown.Set();
            ClosePipes();
            shutdown.Dispose();
        }
    }
}
