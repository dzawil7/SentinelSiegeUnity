🛡️ Sentinel Siege - Unity AI Research Project

Sebuah game 2D Action Platformer yang dikembangkan sebagai media penelitian perbandingan kecerdasan buatan (Artificial Intelligence) pada Boss Battle.

📖 Deskripsi Proyek

Sentinel Siege adalah simulasi pertarungan Boss (Boss Rush) yang dirancang untuk membandingkan efektivitas dan perilaku antara metode Finite State Machine (FSM) konvensional dengan Fuzzy State Machine (FuSM).

Dalam game ini, dua pemain (Local Co-op) bekerja sama untuk mengalahkan "The Sentinel" (Boss) yang perilakunya berubah berdasarkan mode AI yang dipilih sebelum permainan dimulai.

🎯 Tujuan Penelitian

Proyek ini bertujuan untuk menjawab:

Bagaimana perbedaan pola serangan Boss antara logika deterministik (FSM) dan logika fuzzy (FuSM)?

Seberapa responsif Boss AI terhadap perubahan status pemain (jarak, HP, stamina)?

🕹️ Gameplay & Kontrol

Game ini dimainkan oleh 2 pemain dalam satu layar (Local Multiplayer).

🎮 Player 1 (Melee / Tank)

Fokus pada serangan jarak dekat dan menahan agresi Boss.

Aksi

Tombol (Keyboard)

Gerak Kiri/Kanan

A / D

Lompat

W

Serang (Melee)

Spasi / S / F

🏹 Player 2 (Ranged / DPS)

Fokus pada serangan proyektil dari jarak jauh.

Aksi

Tombol (Keyboard)

Gerak Kiri/Kanan

J / L

Lompat

I

Tembak (Ranged)

K

🤖 Boss AI Modes

Boss memiliki 3 mode kecerdasan yang dapat diuji:

Mode 1: FSM Conventional

Menggunakan logika if-else ketat.

Transisi state bersifat kaku dan dapat diprediksi.

Prioritas aksi berdasarkan urutan kode.

Mode 2: FuSM Set A (Basic Fuzzy)

Menggunakan logika fuzzy sederhana berdasarkan Jarak.

Keputusan diambil berdasarkan probabilitas (Weighted Random).

Mode 3: FuSM Set B (Complex Fuzzy)

Menggunakan logika fuzzy kompleks dengan variabel: Jarak, HP Boss, dan Stamina Boss.

Boss dapat melakukan manajemen stamina (Mundur/Shield saat lelah).

Pola serangan lebih dinamis dan sulit diprediksi.

Kemampuan Boss

Idle/Chase: Mengejar pemain terdekat.

Ground Slam: Serangan area jarak dekat.

Laser Beam: Serangan jarak jauh linear.

Shield/Recharge: Memunculkan visual perisai untuk memulihkan stamina (kebal sementara/damage reduction).

Reposition: Melompat untuk berpindah posisi taktis.

🛠️ Instalasi & Cara Menjalankan

Persyaratan Sistem

Unity Editor: Versi 2021.3.4f2 (atau yang lebih baru).

Langkah-langkah

Clone repositori ini:

git clone [https://github.com/username-anda/SentinelSiege.git](https://github.com/username-anda/SentinelSiege.git)


Buka Unity Hub.

Klik Open dan pilih folder hasil clone.

Buka Scene: Assets/Scenes/MainMenu.unity.

Tekan tombol ▶️ Play.

Mainkan di Web (WebGL)

(Opsional: Jika kamu sudah upload ke itch.io, taruh linknya di sini)
Mainkan di itch.io

📂 Struktur Proyek Penting

Assets/Script/BossAI.cs: Otak utama Boss, berisi logika FSM dan FuSM.

Assets/Script/PlayerController.cs: Kontroler untuk Player 1 (Melee) dengan fitur Coyote Time & Jump Buffer.

Assets/Script/Player2Controller.cs: Kontroler untuk Player 2 (Ranged) dengan fitur Coyote Time & Jump Buffer.

Assets/Script/GameManager.cs: Mengatur kondisi menang/kalah dan pemilihan mode AI.

👥 Kredit & Penulis

Dibuat untuk keperluan penelitian akademik.

Developer: dzawil

Aset Visual: Robot Series Base Pack (Open Source).

Catatan: Proyek ini masih dalam tahap pengembangan (Research Prototype).
