# HydraMux ⚡

> **Asynchronous CLI media converter.**

HydraMux is a minimal, thread-safe process execution manager designed to orchestrate simultaneous external workloads. It demonstrates how to handle multi-process lifecycles, IPC streaming, and decoupled telemetry rendering without freezing the main user interface.

---

## 🛠️ Features

* **Thread-Safe Queue:** Job lifecycle management synchronized via low-level primitives.
* **Asynchronous Execution:** Independent background processing driven by a high-velocity task pool.
* **IPC Progress Streaming:** Real-time data capture from redirected sub-process stdout lines.
* **Dynamic Table Telemetry:** Isolated metric engine calculating speed, ETA, and progress arrays.
* **Granular Cancellation:** Target-specific process termination alongside immediate queue flushes.

---

## 📂 Project Structure

```
📂 HydraMux
├── 📁 ConversionManager # Main interface
├── 📁 MockConverter # External worker process emitting raw byte streams
├── 📁 Core
├── 📁 MenuImplementation
├── 📁 MenuLib
├── ⚖️ LICENSE
└── 📝 README.md
```

---

## 🚀 Usage

Launch the main application executable to enter the interactive terminal workspace.

### Navigation Menu
Manage your entire conversion queue using keyboard controls inside the central interactive hub:
* **Add Job:** Append new assets (input/output paths and custom arguments) to the background queue.
* **Monitor Progress (Live):** Switch to a flicker-free telemetry dashboard showing active jobs with their metrics.
* **Cancel One/All:** Target a specific active worker ID or immediately flush the entire system.

### Live Telemetry Dashboard
The monitoring screen dynamically updates metrics for every queued and running thread in a clean, unified view:

```text
=== LIVE MONITOR (Press any key to return to menu) ===

ID   Input        Output       Status       Progress & Metrics
----------------------------------------------------------------------------------------------------
1    movie.mp4    out_1.mkv    Running      [████████░░░░░░░] 53.3% | 24.1 MB/s | ETA: 00:06 | 160.0 MB / 300.0 MB
2    clip.avi     out_2.mp4    Queued       [░░░░░░░░░░░░░░░] 0.0% | Waiting...
----------------------------------------------------------------------------------------------------
Total Jobs: 2 | Running: 1 | Queued: 1
```

---

## License

MIT License (see LICENSE file for details)

