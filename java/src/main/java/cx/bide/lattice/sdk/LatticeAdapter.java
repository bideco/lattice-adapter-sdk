package cx.bide.lattice.sdk;

import java.io.IOException;
import java.io.RandomAccessFile;
import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.function.Consumer;

public final class LatticeAdapter implements AutoCloseable, Runnable {
    private static final int PROTOCOL_VERSION = 2;
    private static final long HEARTBEAT_INTERVAL_MS = 5000L;

    private final AdapterDescriptor descriptor;
    private final Consumer<String> logger;
    private final Queue<LatticeEvent> pending = new ConcurrentLinkedQueue<>();
    private volatile boolean running;
    private volatile boolean connected;
    private volatile RandomAccessFile readPipe;
    private volatile RandomAccessFile writePipe;
    private Thread worker;
    private Thread heartbeat;

    public LatticeAdapter(AdapterDescriptor descriptor, Consumer<String> logger) {
        this.descriptor = descriptor;
        this.logger = logger == null ? message -> { } : logger;
    }

    public boolean isConnected() {
        return connected;
    }

    public AdapterDescriptor descriptor() {
        return descriptor;
    }

    public void start() {
        if (worker != null) return;
        running = true;
        worker = new Thread(this, "Lattice Adapter Client");
        worker.setDaemon(true);
        worker.start();
    }

    @Override
    public void run() {
        while (running) {
            try {
                connect();
                send(LatticeEvent.handshake(PROTOCOL_VERSION, descriptor.slot, descriptor.encodedGameVersion));
                send(LatticeEvent.adapterInfo(descriptor.capabilityMask()));
                startHeartbeat();
                pump();
            } catch (IOException ex) {
                if (running) logger.accept("Lattice adapter disconnected: " + ex.getMessage());
            } finally {
                connected = false;
                closePipes();
                stopHeartbeat();
            }
            sleep(1000L);
        }
    }

    public LatticeEvent pollEvent() {
        return pending.poll();
    }

    public void drainEvents(Consumer<LatticeEvent> consumer) {
        LatticeEvent event;
        while ((event = pending.poll()) != null) {
            consumer.accept(event);
        }
    }

    public synchronized void send(LatticeEvent event) throws IOException {
        if (writePipe == null) return;
        writePipe.write(event.encode());
    }

    private void connect() throws IOException {
        String base = "\\\\.\\pipe\\latticework-" + descriptor.slot;
        readPipe = new RandomAccessFile(base + "-out", "rw");
        writePipe = new RandomAccessFile(base + "-in", "rw");
        connected = true;
        logger.accept("Connected to Lattice Weaver as P" + descriptor.slot);
    }

    private void pump() throws IOException {
        byte[] data = new byte[LatticeEvent.SIZE];
        while (running && readPipe != null) {
            readFully(readPipe, data);
            LatticeEvent event = LatticeEvent.decode(data);
            if (event != null) {
                pending.add(event);
            }
        }
    }

    private static void readFully(RandomAccessFile file, byte[] data) throws IOException {
        int offset = 0;
        while (offset < data.length) {
            int read = file.read(data, offset, data.length - offset);
            if (read < 0) throw new IOException("Pipe closed");
            offset += read;
        }
    }

    private void startHeartbeat() {
        stopHeartbeat();
        heartbeat = new Thread(() -> {
            while (running && connected) {
                try {
                    send(LatticeEvent.heartbeat());
                } catch (IOException ignored) {
                    return;
                }
                sleep(HEARTBEAT_INTERVAL_MS);
            }
        }, "Lattice Adapter Heartbeat");
        heartbeat.setDaemon(true);
        heartbeat.start();
    }

    private void stopHeartbeat() {
        if (heartbeat != null) {
            heartbeat.interrupt();
            heartbeat = null;
        }
    }

    private static void sleep(long millis) {
        try {
            Thread.sleep(millis);
        } catch (InterruptedException ignored) {
            Thread.currentThread().interrupt();
        }
    }

    private void closePipes() {
        closeQuietly(readPipe);
        closeQuietly(writePipe);
        readPipe = null;
        writePipe = null;
    }

    private static void closeQuietly(RandomAccessFile file) {
        if (file == null) return;
        try {
            file.close();
        } catch (IOException ignored) {
        }
    }

    @Override
    public void close() {
        running = false;
        stopHeartbeat();
        closePipes();
    }
}
