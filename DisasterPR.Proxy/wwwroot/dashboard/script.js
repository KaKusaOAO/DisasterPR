function varInt(buf) {
    var i = 0;
    var val = 0;
    var pos = 0;
    var b = 0;

    while (true) {
        var read = buf[i++];
        if (read == null) throw new Error();

        b = read;
        val |= (b & 0x7f) << pos;
        if ((b & 0x80) == 0) break;

        pos += 7;
        if (pos >= 32) throw new Error("VarInt too big");
    }

    return val;
}

function varIntBuf(v) {
    var buf = [];
    while (true) {
        if ((v & ~0x7f) == 0) {
            buf.push(Math.min(0xff, Math.max(0, v)));
            return buf;
        }

        buf.push(Math.min(0xff, Math.max(0, ((v & 0x7f) | 0x80))));
        v >>>= 7;
    }
}

function createPayload(buf) {
    return [
        ...varIntBuf(buf.length),
        ...buf
    ];
}

(() => {
    /** @type {HTMLPreElement} */
    var output = document.getElementById("output");

    /** @type {HTMLInputElement} */
    var input = document.getElementById("cmd-input");

    var gateway = new WebSocket("ws://127.0.0.1:5221/gateway");
    gateway.onopen = _ => {
        gateway.onmessage = e => {
            console.log(e.data);
        };
        
        // Handshake
        gateway.send(new Int8Array(createPayload([
            ...varIntBuf(0), // ServerboundHelloPacket
            ...varIntBuf(1), // protocol version
        ])));

        // Login (state changed)
        var name = "米糰";
        var encoded = new TextEncoder().encode(name);
        gateway.send(new Int8Array(createPayload([
            ...varIntBuf(0), // ServerboundLoginPacket
            ...varIntBuf(encoded.length),
            ...encoded
        ])));
    };

    var ws = new WebSocket("ws://127.0.0.1:5221/api/dashboard");
    ws.onmessage = e => {
        var data = JSON.parse(e.data);
        var elem = document.createElement("span");
        var t = (s => {
            var result = document.createElement("span");
            result.innerText = s;
            return result;
        });

        elem.appendChild(t(data.timestamp + " - "));
        var tag = t(`[${data.tag}]`);
        tag.style.color = data.color;
        elem.appendChild(tag);

        var pad = `${data.timestamp} - [${data.tag}] `;
        pad = pad.split("").map(_ => " ").join("");

        elem.appendChild(t(` ${data.content.split("\n").join(pad)}\n`));
        output.appendChild(elem);

        output.parentElement.parentElement.scrollTo(0, output.scrollHeight);

        while (output.scrollHeight >= window.innerHeight * 10) {
            output.children.item(0)?.remove();
        }
    };
    ws.onclose = e => {
        location.href = location.href;
    };

    input.addEventListener("keydown", e => {
        if (e.key != "Enter") return;
        if (input.value.trim().length == 0) return;

        ws.send(JSON.stringify({
            command: input.value.trim()
        }));
        input.value = "";
    });
})();