"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var hljs = require("highlight.js");
var maybe = function (f) {
    try {
        return f();
    }
    catch (e) {
        return false;
    }
};
// Highlight with given language.
var highlight = function (code, lang) {
    return maybe(function () { return hljs.highlight(lang, code, true).value; }) || '';
};
// Highlight with given language or automatically.
var highlightAuto = function (code, lang) {
    return lang
        ? highlight(code, lang)
        : maybe(function () { return hljs.highlightAuto(code).value; }) || '';
};
// Wrap a render function to add `hljs` class to code blocks.
var wrap = function (render) {
    return function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i] = arguments[_i];
        }
        return render.apply(this, args)
            .replace('<code class="', '<code class="hljs ')
            .replace('<code>', '<code class="hljs">');
    };
};
function highlightjs(md, opts) {
    if (opts == undefined) {
        opts = highlightjs.defaults;
    }
    md.options.highlight = opts.auto ? highlightAuto : highlight;
    md.renderer.rules.fence = wrap(md.renderer.rules.fence);
    if (opts.code) {
        md.renderer.rules.code_block = wrap(md.renderer.rules.code_block);
    }
}
exports.default = highlightjs;
highlightjs.defaults = {
    auto: true,
    code: true
};
//# sourceMappingURL=markdown-it-code.js.map