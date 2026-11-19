let audioCtx = null;

export async function playAchievementFanfare() {
    try {
        const AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) return;

        if (!audioCtx) {
            audioCtx = new AudioCtx();
        }

        if (audioCtx.state === 'suspended') {
            await audioCtx.resume();
        }

        const now = audioCtx.currentTime;
        const notes = [
            { f: 523.25, start: 0.00, dur: 0.15 },
            { f: 659.25, start: 0.12, dur: 0.15 },
            { f: 783.99, start: 0.24, dur: 0.20 },
            { f: 1046.50, start: 0.40, dur: 0.70 }
        ];

        for (const n of notes) {
            const osc = audioCtx.createOscillator();
            const gain = audioCtx.createGain();
            osc.type = 'triangle';
            osc.frequency.setValueAtTime(n.f, now + n.start);

            gain.gain.setValueAtTime(0.0001, now + n.start);
            gain.gain.linearRampToValueAtTime(0.35, now + n.start + 0.03);
            gain.gain.exponentialRampToValueAtTime(0.0001, now + n.start + n.dur);

            osc.connect(gain);
            gain.connect(audioCtx.destination);

            osc.start(now + n.start);
            osc.stop(now + n.start + n.dur);
        }
    } catch {
        // Ignore audio playback failures in restricted or unsupported environments
    }
}

export async function downloadFileFromStream(fileName, contentStreamReference, contentType) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName ?? '';
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export function showModalDialog(dialogEl, lightDismiss) {
    if (!dialogEl) return;

    if (!dialogEl.open) {
        dialogEl.showModal();
    }

    if (lightDismiss && !('closedBy' in HTMLDialogElement.prototype) && !dialogEl._lightDismissBound) {
        dialogEl._lightDismissBound = true;
        dialogEl.addEventListener('click', (e) => {
            if (e.clientX === 0 && e.clientY === 0) return;

            const rect = dialogEl.getBoundingClientRect();
            const isInDialog =
                e.clientX >= rect.left &&
                e.clientX <= rect.right &&
                e.clientY >= rect.top &&
                e.clientY <= rect.bottom;

            if (!isInDialog && dialogEl.open) {
                dialogEl.close();
            }
        });
    }
}

export function closeDialog(dialogEl) {
    if (dialogEl?.open) {
        dialogEl.close();
    }
}

export function focusElement(el) {
    el?.focus();
}

export function setElementInert(selector, isInert) {
    const el = document.querySelector(selector);
    if (el) {
        el.inert = !!isInert;
    }
}
