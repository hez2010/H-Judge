import * as React from 'react';
import { ensureLoading } from '../../utils/scriptLoader';
import { nextTick } from 'q';
import { Placeholder } from 'semantic-ui-react';

interface CodeEditorState {
  editorLoaded: boolean
}

interface CodeEditorProps {
  language: string,
  onBlur?: () => void,
  onChange?: (e: Object) => void,
  onChangeSelectionStyle?: (data: Object) => void,
  onChangeSession?: (e: Object) => void,
  onCopy?: (text: String) => void,
  onFocus?: () => void,
  onPaste?: (e: Object) => void
}

export default class CodeEditor extends React.Component<CodeEditorProps, CodeEditorState> {
  constructor(props: CodeEditorProps) {
    super(props);
    this.loadEditor = this.loadEditor.bind(this);
    this.loadEditor(this.props.language);

    this.state = {
      editorLoaded: false
    };
  }

  uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  private uuid: string = this.uuidv4();
  private editor: any = null;

  public getInstance() {
    return this.editor;
  }

  loadEditor(lang: string) {
    ensureLoading('ace', '/lib/ace/ace.js', () => {
      this.setState({ editorLoaded: true } as CodeEditorState);
      nextTick(() => {
        let w = window as any;
        this.editor = w.ace.edit(`code-editor-${this.uuid}`);
        if (this.props.onBlur) this.editor.on('blur', this.props.onBlur);
        if (this.props.onChange) this.editor.on('change', this.props.onChange);
        if (this.props.onChangeSelectionStyle) this.editor.on('changeSelectionStyle', this.props.onChangeSelectionStyle);
        if (this.props.onChangeSession) this.editor.on('changeSession', this.props.onChangeSession);
        if (this.props.onCopy) this.editor.on('copy', this.props.onCopy);
        if (this.props.onFocus) this.editor.on('focus', this.props.onFocus);
        if (this.props.onPaste) this.editor.on('paste', this.props.onPaste);
        
        this.editor.setTheme('ace/theme/tomorrow');
        this.editor.session.setMode(`ace/mode/${lang}`);
      });
    });
  }

  componentWillUpdate(nextProps: CodeEditorProps, nextState: CodeEditorState) {
    if (this.editor) this.editor.session.setMode(`ace/mode/${nextProps.language}`);
  }

  render() {
    let placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;

    let editor = this.state.editorLoaded ? <>
      <div id={`code-editor-${this.uuid}`} style={{ width: '100%', height: '100%' }}></div>
    </> : placeHolder;

    return editor;
  }
}