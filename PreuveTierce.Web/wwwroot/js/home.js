function closeVerifyPresence() {
    const card = document.getElementById('verify-presence-card');
    if (!card) return;

    card.classList.add('opacity-0', 'scale-95');

    setTimeout(() => {
        card.remove();

        // Optionnel : scroll vers la zone d’upload
        const uploadZone = document.getElementById('view-home');
        uploadZone?.scrollIntoView({ behavior: 'smooth' });

    }, 200);
}

async function verifyPresence() {
    const serial = document.getElementById('certificateSerialInput').value;

    const response = await fetch('/Home/VerifyPresence', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `certificateSerial=${encodeURIComponent(serial)}`
    });
    alert('BRAVO !');
    const html = await response.text();
    document.getElementById('verify-presence-result').innerHTML = html;
}
function downloadCertificate(hash) {
    window.location.href = `/Home/DownloadPublicCertificate?hash=${hash}`;
}
function downloadTimestamp(serial) {
    window.location.href = `/Home/DownloadTimestamp?serial=${serial}`;
}

// --- GESTION DU DRAG & DROP ---

document.addEventListener('DOMContentLoaded', () => {
    let currentHash = null;
    const dropzone = document.getElementById('dropzone');
    const fileInput = document.getElementById('fileInput');
    const verifyBtn = document.getElementById('verifyBtn');
    const btnText = document.getElementById('btnText');
    const btnSpinner = document.getElementById('btnSpinner');
    const btnHint = document.getElementById('btnHint');
    const resultArea = document.getElementById('resultArea');
    // --- 1. GESTION DES CLICS ---
    dropzone.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) handleFile(e.target.files[0]);
    });

    // --- 2. GESTION DU DRAG & DROP (MODERNE) ---
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropzone.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
        }, false);
    });

    dropzone.addEventListener('dragenter', () => {
        dropzone.classList.add('bg-blue-50', 'border-primary', 'scale-[1.02]');
    });

    dropzone.addEventListener('dragleave', () => {
        dropzone.classList.remove('bg-blue-50', 'border-primary', 'scale-[1.02]');
    });

    dropzone.addEventListener('drop', (e) => {
        dropzone.classList.remove('scale-[1.02]');
        const files = e.dataTransfer.files;
        if (files.length > 0) handleFile(files[0]);
    });
    verifyBtn.addEventListener('click', async () => {
        if (!currentHash) return;

        verifyBtn.disabled = true;
        btnSpinner.classList.remove('hidden');
        btnText.textContent = "Recherche dans la Blockchain...";
        resultArea.innerHTML = '';
        resultArea.classList.add('hidden');

        try {
            const response = await fetch(`/Home/VerifyDocument?hash=${currentHash}`);
            const data = await response.json();
            displayResult(data);

        } catch (error) {
            displayResult({ success: false, message: "Erreur de connexion au serveur." });
        } finally {
            verifyBtn.disabled = false;
            btnSpinner.classList.add('hidden');
            btnText.textContent = "Vérifier l'authenticité";
        }
    });
    function displayResult(data) {
        resultArea.classList.remove('hidden');

        if (data.success) {
            resultArea.innerHTML = `<div class="mt-6 bg-green-50 border border-green-200 rounded-lg p-5 space-y-3 animate-fade-in">
                <div class="flex items-start">
                    <svg class="h-6 w-6 text-green-600 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                    </svg>
                    <div class="flex-1">
                        <h3 class="text-sm font-semibold text-green-800">
                            Document certifié authentique
                        </h3>
                        <p class="text-sm text-green-700 mt-1">
                            Cette version du document correspond exactement à la preuve enregistrée.
                        </p>
                    </div>
                </div>

                <div class="m-3 text-xs text-gray-600 space-y-1 border-t border-green-200 pt-3">
                    <p class="flex gap-3"><strong>Horodatage (UTC) :</strong> ${data.date}</p>
                    <p class="flex gap-3"><strong>Algorithme :</strong> SHA-256</p>
                    <p class="flex gap-3"><strong>Référence de preuve :</strong> <span class="font-mono">${data.serial}</span></p>
                    <p class="flex gap-3 break-all">
                        <strong>Empreinte :</strong>
                        ${currentHash}
                    </p>
                </div>
            </div>

            <div class="mt-4 flex flex-wrap gap-3 justify-center items-center">
                <button onclick="downloadCertificate('${currentHash}')" 
                        class="cursor-pointer  bg-primary text-white px-4 py-2 rounded text-sm font-medium hover:bg-primary/90 transition flex items-center shadow-sm">
                    <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m0 0l-4-4m4 4l4-4M4 20h16" />
                    </svg>
                    Télécharger l’attestation PDF
                </button>
                ${data.hasTimestampToken ? `
                <button onclick="downloadTimestamp('${data.serial}')"
                        class="cursor-pointer px-4 py-2 text-sm font-medium rounded-md
                        border border-blue-300 text-blue-700 hover:bg-blue-50 transition">
                    Télécharger le fichier d’horodatage (.tsr)
                </button>
                ` : ''}
                <button onclick="window.location.reload()" class="cursor-pointer  text-sm text-gray-600 hover:text-primary underline">
                    Vérifier un autre document
                </button>
            </div>
        `;
        } else {
            // ❌ CAS : ERREUR (Document modifié ou inconnu)
            resultArea.innerHTML = `
            <div class="mt-6 bg-red-50 border border-red-200 rounded-lg p-5 flex items-start animate-shake">
                <svg class="h-6 w-6 text-red-600 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <div>
                    <h3 class="text-sm font-semibold text-red-800">Échec de la vérification</h3>
                    <p class="text-sm text-red-700 mt-1">${data.message}</p>
                    <button onclick="window.location.reload()" class="cursor-pointer  mt-3 text-xs text-red-800 font-bold uppercase tracking-wider hover:underline">
                        Réessayer avec un autre fichier
                    </button>
                </div>
            </div>
        `;
        }
    }
    // --- 3. TRAITEMENT DU FICHIER ET HACHAGE ---
    async function handleFile(file) {
        updateUISelected(file.name);
        verifyBtn.disabled = true;
        btnText.textContent = "Calcul de l'empreinte...";
        resultArea.classList.add('hidden');
        try {
            console.log("Calcul du Hash pour :", file.name);
            const hash = await computeSHA256(file);
            currentHash = hash;
            verifyBtn.disabled = false;
            btnText.textContent = "Vérifier l'authenticité";
            btnHint.textContent = "Empreinte générée : " + hash.substring(0, 15) + "...";
            btnHint.classList.add('text-primary');
        } catch (err) {
            console.error("Erreur de hachage:", err);
        }
    }

    // --- FONCTION DE HACHAGE SHA-256 (CLIENT-SIDE) ---
    async function computeSHA256(file) {
        const buffer = await file.arrayBuffer();
        const hashBuffer = await crypto.subtle.digest('SHA-256', buffer);
        const hashArray = Array.from(new Uint8Array(hashBuffer));
        return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    }

    function updateUISelected(fileName) {
        const text = document.getElementById('dropzoneText');
        const icon = document.getElementById('uploadIcon');
        const hint = document.getElementById('dropzoneHint');

        text.innerHTML = `Fichier prêt : <span class="text-primary font-bold">${fileName}</span>`;
        hint.classList.add('hidden');
        icon.innerHTML = `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />`;
        icon.classList.replace('text-gray-400', 'text-green-500');
        dropzone.classList.add('border-solid', 'bg-green-50/20');
        dropzone.classList.remove('border-dashed');
    }
});