export default function texmath(md: any, options: any) {
  let delimiters = options && options.delimiters || 'dollars',
    macros = options && options.macros;

  if (delimiters in texmath.rules) {
    for (let rule of (texmath as any).rules[delimiters].inline) {
      md.inline.ruler.before('escape', rule.name, texmath.inline(rule));  // ! important
      md.renderer.rules[rule.name] = (tokens: any, idx: any) => rule.tmpl.replace(/\$1/, texmath.render(tokens[idx].content, false, macros));
    }

    for (let rule of (texmath as any).rules[delimiters].block) {
      md.block.ruler.before('fence', rule.name, texmath.block(rule));
      md.renderer.rules[rule.name] = (tokens: any, idx: any) => rule.tmpl.replace(/\$2/, tokens[idx].info)  // equation number .. ?
        .replace(/\$1/, texmath.render(tokens[idx].content, true, macros));
    }
  }
}

texmath.applyRule = function (rule: any, str: any, beg: any, inBlockquote?: any) {
  let pre, match, post;
  rule.rex.lastIndex = beg;

  pre = str.startsWith(rule.tag, beg) && (!rule.pre || rule.pre(str, beg));
  match = pre && rule.rex.exec(str);
  if (match) {
    match.lastIndex = rule.rex.lastIndex;
    post = (!rule.post || rule.post(str, match.lastIndex - 1))  // valid post-condition
      && (!inBlockquote || !match[1].includes('\n'));       // remove evil blockquote bug (https://github.com/goessner/mdmath/issues/50)
  }
  rule.rex.lastIndex = 0;

  return post && match;
}

// texmath.inline = (rule) => dollar;  // just for testing ..

texmath.inline = (rule: any) =>
  function (state: any, silent: any) {
    let res = texmath.applyRule(rule, state.src, state.pos);
    if (res) {
      if (!silent) {
        let token = state.push(rule.name, 'math', 0);
        token.content = res[1];  // group 1 from regex ..
        token.markup = rule.tag;
      }
      state.pos = res.lastIndex;
    }
    return !!res;
  }

texmath.block = (rule: any) =>
  function (state: any, begLine: any, endLine: any, silent: any) {
    let res = texmath.applyRule(rule, state.src, state.bMarks[begLine] + state.tShift[begLine], state.parentType === 'blockquote');
    if (res) {
      if (!silent) {
        let token = state.push(rule.name, 'math', 0);
        token.block = true;
        token.content = res[1];
        token.info = res[res.length - 1];
        token.markup = rule.tag;
      }
      for (let line = begLine, endpos = res.lastIndex - 1; line < endLine; line++)
        if (endpos >= state.bMarks[line] && endpos <= state.eMarks[line]) { // line for end of block math found ...
          state.line = line + 1;
          break;
        }
      state.pos = res.lastIndex;
    }
    return !!res;
  }

texmath.render = function (tex: any, displayMode: any, macros: any) {
  let res;
  try {
    res = (texmath as any).katex.renderToString(tex, { throwOnError: false, displayMode, macros });
  }
  catch (err) {
    res = tex + ": " + err.message.replace("<", "&lt;");
  }
  return res;
}

texmath.use = function (katex: any) {  // math renderer used ...
  (texmath as any).katex = katex;       // ... katex solely at current ...
  return texmath;
}

texmath.$_pre = (str: any, beg: any) => {
  let prv = beg > 0 ? str[beg - 1].charCodeAt(0) : false;
  return !prv || prv !== 0x5c                // no backslash,
    && (prv < 0x30 || prv > 0x39); // no decimal digit .. before opening '$'
}
texmath.$_post = (str: any, end: any) => {
  let nxt = str[end + 1] && str[end + 1].charCodeAt(0);
  return !nxt || nxt < 0x30 || nxt > 0x39;   // no decimal digit .. after closing '$'
}

texmath.rules = {
  brackets: {
    inline: [
      {
        name: 'math_inline',
        rex: /\\\((.+?)\\\)/gy,
        tmpl: '<eq>$1</eq>',
        tag: '\\('
      }
    ],
    block: [
      {
        name: 'math_block_eqno',
        rex: /\\\[(((?!\\\]|\\\[)[\s\S])+?)\\\]\s*?\(([^)$\r\n]+?)\)/gmy,
        tmpl: '<section class="eqno"><eqn>$1</eqn><span>($2)</span></section>',
        tag: '\\['
      },
      {
        name: 'math_block',
        rex: /\\\[([\s\S]+?)\\\]/gmy,
        tmpl: '<section><eqn>$1</eqn></section>',
        tag: '\\['
      }
    ]
  },
  gitlab: {
    inline: [
      {
        name: 'math_inline',
        rex: /\$`(.+?)`\$/gy,
        tmpl: '<eq>$1</eq>',
        tag: '$`'
      }
    ],
    block: [
      {
        name: 'math_block_eqno',
        rex: /`{3}math\s+?([^`]+?)\s+?`{3}\s*?\(([^)$\r\n]+?)\)/gmy,
        tmpl: '<section class="eqno"><eqn>$1</eqn><span>($2)</span></section>',
        tag: '```math'
      },
      {
        name: 'math_block',
        rex: /`{3}math\s+?([^`]+?)\s+?`{3}/gmy,
        tmpl: '<section><eqn>$1</eqn></section>',
        tag: '```math'
      }
    ]
  },
  kramdown: {
    inline: [
      {
        name: 'math_inline',
        rex: /\${2}([^$\r\n]*?)\${2}/gy,
        tmpl: '<eq>$1</eq>',
        tag: '$$'
      }
    ],
    block: [
      {
        name: 'math_block_eqno',
        rex: /\${2}([^$]*?)\${2}\s*?\(([^)$\r\n]+?)\)/gmy,
        tmpl: '<section class="eqno"><eqn>$1</eqn><span>($2)</span></section>',
        tag: '$$'
      },
      {
        name: 'math_block',
        rex: /\${2}([^$]*?)\${2}/gmy,
        tmpl: '<section><eqn>$1</eqn></section>',
        tag: '$$'
      }
    ]
  },
  dollars: {
    inline: [
      {
        name: 'math_inline',
        rex: /\$(\S[^$\r\n]*?[^\s\\]{1}?)\$/gy,
        tmpl: '<eq>$1</eq>',
        tag: '$',
        pre: texmath.$_pre,
        post: texmath.$_post
      },
      {
        name: 'math_single',
        rex: /\$([^$\s\\]{1}?)\$/gy,
        tmpl: '<eq>$1</eq>',
        tag: '$',
        pre: texmath.$_pre,
        post: texmath.$_post
      }
    ],
    block: [
      {
        name: 'math_block_eqno',
        rex: /\${2}([^$]*?)\${2}\s*?\(([^)$\r\n]+?)\)/gmy,
        tmpl: '<section class="eqno"><eqn>$1</eqn><span>($2)</span></section>',
        tag: '$$'
      },
      {
        name: 'math_block',
        rex: /\${2}([^$]*?)\${2}/gmy,
        tmpl: '<section><eqn>$1</eqn></section>',
        tag: '$$'
      }
    ]
  }
};
