import * as hljs from 'highlight.js';
import markdownit from 'markdown-it';

const maybe = (f: any) => {
  try {
    return f();
  }
  catch (e) {
    return false;
  }
};

// Highlight with given language.
const highlight = (code: string, lang: string) =>
  maybe(() => hljs.highlight(lang, code, true).value) || '';

// Highlight with given language or automatically.
const highlightAuto = (code: string, lang: string) =>
  lang
    ? highlight(code, lang)
    : maybe(() => hljs.highlightAuto(code).value) || '';

// Wrap a render function to add `hljs` class to code blocks.
const wrap = (render: Function, thisArg: any) => function (...args: any) {
  return render.apply(thisArg, args)
    .replace(/\<code class="/g, '<code class="hljs ')
    .replace(/\<code>/g, '<code class="hljs">');
}
interface Option {
  auto: boolean, code: boolean
}
export default function highlightjs(md: markdownit, opts?: Option) {
  if (!opts) {
    opts = highlightjs.defaults;
  }

  md.set({ highlight: opts.auto ? highlightAuto : highlight });
  md.renderer.rules.fence = wrap(md.renderer.rules.fence, md);

  if (opts.code) {
    md.renderer.rules.code_block = wrap(md.renderer.rules.code_block, md);
  }
}

highlightjs.defaults = {
  auto: true,
  code: true
};
