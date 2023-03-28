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

function jsonToHtml(json) {
    if (json instanceof Array) {
        return json.map(j => jsonToHtml(j)).join("");
    }

    var content = "";
    if (json.text) {
        content = json.text.replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    if (json.translate) {
        /** @type {string} */
        var fm = json.translate.replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var regex = /%(?:(?:(\d*?)\$)?)s/g;
        var i = 0;
        content = fm.replace(regex, s => {
            var matches = s.matchAll(regex);
            var v = matches.next().value[1];
            if (v == null) v = i++;
            return jsonToHtml(json.with.map(e => {
                e.color ??= json.color;
                return e;
            })[v]);
        });
    }

    if (json.extra) {
        content += jsonToHtml(json.extra.map(e => {
            e.color ??= json.color;
            return e;
        }));
    }

    var color = json.color ?? "white";
    return `<span style="color: ${color};">${content}</span>`;
}

function jsonToPlain(json) {
    if (json instanceof Array) {
        return json.map(j => jsonToPlain(j)).join("");
    }

    var content = "";
    if (json.text) {
        content = json.text;
    }

    if (json.translate) {
        /** @type {string} */
        var fm = json.translate;
        var regex = /%(?:(?:(\d*?)\$)?)s/g;
        var i = 0;
        content = fm.replace(regex, s => {
            var matches = s.matchAll(regex);
            var v = matches.next().value[1];
            if (v == null) v = i++;
            return jsonToPlain(json.with[v]);
        });
    }

    if (json.extra) {
        content += jsonToPlain(json.extra);
    }

    return content;
}

(() => {
    /** @type {HTMLPreElement} */
    var output = document.getElementById("output");

    /** @type {HTMLInputElement} */
    var input = document.getElementById("cmd-input");

    var ws = new WebSocket("ws://127.0.0.1:5221/api/dashboard");
    ws.onmessage = e => {
        var data = JSON.parse(e.data);
        var elem = document.createElement("span");
        var t = (s => {
            var result = document.createElement("span");
            result.innerText = s;
            return result;
        });

        var h = (s => {
            var result = document.createElement("span");
            result.innerHTML = s;
            return result;
        });

        elem.appendChild(t(data.timestamp + " - "));
        data.tag.color = data.color;
        var tagJson = {
            translate: "[%s] ",
            with: [data.tag],
            color: data.tag.color
        };
        elem.appendChild(h(jsonToHtml(tagJson)));

        var pad = `${data.timestamp} - ${jsonToPlain(tagJson)}`;
        pad = pad.split("").map(_ => " ").join("");

        elem.appendChild(h(jsonToHtml(data.content).split("\n").join("\n" + pad) + "\n"));
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