import * as React from 'react';
import md from 'markdown-it';
import katex from 'katex';
import texmath from '../../extensions/markdown-it-math';
import hljs from '../../extensions/markdown-it-code';

interface MarkdownViewerProps {
  content: string
}

export default class MarkdownViewer extends React.Component<MarkdownViewerProps> {
  constructor(props: MarkdownViewerProps) {
    super(props);

    this.markdown = new md({ html: true })
      .use(texmath.use(katex), { delimiters: 'brackets' })
      .use(texmath.use(katex), { delimiters: 'dollars' })
      .use(hljs);
  }

  private markdown: any;

  render() {
    return <div dangerouslySetInnerHTML={{ __html: this.markdown.render(this.props.content) }}></div>;
  }
}
