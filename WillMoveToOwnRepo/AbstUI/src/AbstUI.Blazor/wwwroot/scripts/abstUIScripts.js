export class abstCanvas {
    static createCanvas(width, height) {
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        return canvas;
    }

    static disposeCanvas(canvas) {
        if (canvas && canvas.remove) {
            canvas.remove();
        }
    }

    static addCanvasToBody(canvas) {
        document.body.appendChild(canvas);
    }

    static addCanvasToElement(element, canvas) {
        if (element && canvas) {
            element.appendChild(canvas);
        }
    }

    static setCanvasOffset(canvas, x, y) {
        if (!canvas) return;
        canvas.style.position = 'absolute';
        canvas.style.left = `${-x}px`;
        canvas.style.top = `${-y}px`;
    }

    static setCanvasVisible(canvas, visible) {
        canvas.style.display = visible ? 'block' : 'none';
    }

    static getContext(canvas, pixilated) {
        const ctx = canvas.getContext('2d');
        if (pixilated) {
            ctx.imageSmoothingEnabled = false;
        }
        return ctx;
    }

    static clear(ctx, color, width, height) {
        ctx.fillStyle = color;
        ctx.fillRect(0, 0, width, height);
    }

    static setPixel(ctx, x, y, color) {
        ctx.fillStyle = color;
        ctx.fillRect(x, y, 1, 1);
    }

    static drawLine(ctx, x1, y1, x2, y2, color, width) {
        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.strokeStyle = color;
        ctx.lineWidth = width;
        ctx.stroke();
    }

    static drawRect(ctx, x, y, w, h, color, filled, width) {
        if (filled) {
            ctx.fillStyle = color;
            ctx.fillRect(x, y, w, h);
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.strokeRect(x, y, w, h);
        }
    }

    static drawCircle(ctx, x, y, radius, color, filled, width) {
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, Math.PI * 2);
        if (filled) {
            ctx.fillStyle = color;
            ctx.fill();
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.stroke();
        }
    }

    static drawArc(ctx, x, y, radius, startDeg, endDeg, color, width) {
        ctx.beginPath();
        ctx.arc(x, y, radius, startDeg * Math.PI / 180, endDeg * Math.PI / 180);
        ctx.strokeStyle = color;
        ctx.lineWidth = width;
        ctx.stroke();
    }

    static drawPolygon(ctx, points, color, filled, width) {
        if (points.length < 4) return;
        ctx.beginPath();
        ctx.moveTo(points[0], points[1]);
        for (let i = 2; i < points.length; i += 2) {
            ctx.lineTo(points[i], points[i + 1]);
        }
        ctx.closePath();
        if (filled) {
            ctx.fillStyle = color;
            ctx.fill();
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.stroke();
        }
    }

    static drawText(ctx, x, y, text, font, color, fontSize, alignment, letterSpacing = 0) {
        ctx.fillStyle = color;
        ctx.font = font || (fontSize + 'px sans-serif');
        ctx.textAlign = alignment;
        const lines = text.split('\n');
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            if (letterSpacing !== 0) {
                let lx = x;
                for (const ch of line) {
                    ctx.fillText(ch, lx, y + i * fontSize);
                    lx += ctx.measureText(ch).width + letterSpacing;
                }
            } else {
                ctx.fillText(line, x, y + i * fontSize);
            }
        }
    }

    static drawPictureData(ctx, data, width, height, x, y) {
        const imgData = new ImageData(new Uint8ClampedArray(data), width, height);
        ctx.putImageData(imgData, x, y);
    }

    static getImageData(ctx, width, height) {
        return ctx.getImageData(0, 0, width, height).data;
    }

    static setGlobalAlpha(ctx, alpha) {
        ctx.globalAlpha = alpha;
    }
}

export class AbstScrollContainer {
     static getScrollPosition(elementId) {
        let element = document.getElementById(elementId);
         if (element == null) return null;
            return {
                scrollTop: element.scrollTop,
                scrollLeft: element.scrollLeft,
                scrollHeight: element.scrollHeight,
                clientHeight: element.clientHeight
            };
        }
}

export class AbstUIKey {
    static setCursor(cursor) {
        document.body.style.cursor = cursor;
    }
}

export class AbstUIWindow {
    static showBootstrapModal(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.show();
    }

    static hideBootstrapModal(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.hide();
    }

    static showBootstrapConfirm(title, message) {
        return new Promise(resolve => {
            const id = `abstui-confirm-${crypto.randomUUID()}`;
            document.body.insertAdjacentHTML('beforeend', `
<div class="modal" tabindex="-1" id="${id}">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">${title}</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body"><p>${message}</p></div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="button" class="btn btn-primary" id="${id}-ok">OK</button>
      </div>
    </div>
  </div>
</div>`);
            const modalEl = document.getElementById(id);
            const modal = new bootstrap.Modal(modalEl);
            let confirmed = false;
            document.getElementById(`${id}-ok`).addEventListener('click', () => {
                confirmed = true;
                resolve(true);
                modal.hide();
            });
            modalEl.addEventListener('hidden.bs.modal', () => {
                if (!confirmed) resolve(false);
                modalEl.remove();
            });
            modal.show();
        });
    }

    static showBootstrapToast(message, type) {
        const container = document.getElementById('abstui-toast-container') ?? (() => {
            const div = document.createElement('div');
            div.id = 'abstui-toast-container';
            div.className = 'toast-container position-fixed top-0 end-0 p-3';
            document.body.appendChild(div);
            return div;
        })();
        const id = `abstui-toast-${crypto.randomUUID()}`;
        const cls = type === 'error' ? 'bg-danger text-white' : type === 'info' ? 'bg-info text-white' : 'bg-warning text-dark';
        container.insertAdjacentHTML('beforeend', `
<div id="${id}" class="toast ${cls}" role="alert" aria-live="assertive" aria-atomic="true">
  <div class="d-flex">
    <div class="toast-body">${message}</div>
    <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
  </div>
</div>`);
        const toastEl = document.getElementById(id);
        const toast = new bootstrap.Toast(toastEl);
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
        toast.show();
    }
}

export class abstMedia {
    static createVideo(id, url, dotNetHelper) {
        const video = document.createElement('video');
        video.id = id;
        video.src = url;
        video.addEventListener('ended', () => dotNetHelper.invokeMethodAsync('OnVideoEnded'));
        video.load();
        return video;
    }
    static playVideo(video) { video.play(); }
    static pauseVideo(video) { video.pause(); }
    static stopVideo(video) { video.pause(); video.currentTime = 0; }
    static seekVideo(video, seconds) { video.currentTime = seconds; }
    static getDuration(video) { return isNaN(video.duration) ? 0 : video.duration * 1000.0; }
    static getCurrentTime(video) { return video.currentTime * 1000.0; }

    static createAudio(id, dotNetHelper) {
        const audio = new Audio();
        audio.id = id;
        audio.addEventListener('ended', () => dotNetHelper.invokeMethodAsync('OnSoundEnded'));
        return audio;
    }
    static playAudio(audio, url) { audio.src = url; audio.play(); }
    static pauseAudio(audio) { audio.pause(); }
    static stopAudio(audio) { audio.pause(); audio.currentTime = 0; }
    static resumeAudio(audio) { audio.play(); }
    static seekAudio(audio, seconds) { audio.currentTime = seconds; }
    static getCurrentTimeAudio(audio) { return audio.currentTime * 1000.0; }
    static setVolumeAudio(audio, volume) { audio.volume = volume; }
    static beep() {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        osc.type = 'square';
        osc.frequency.value = 440;
        osc.connect(ctx.destination);
        osc.start();
        osc.stop(ctx.currentTime + 0.2);
    }
}
