import * as React from 'react';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { Get } from '../../utils/requestHelper';
import CodeEditor from '../editor/code';
import { Placeholder, Tab } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';

interface ProblemEditState {
  problem: ProblemEditModel
}

interface ProblemEditProps extends CommonProps {
  problemId?: number
}

interface DataPoint {
  stdInFile: string,
  stdOutFile: string,
  timeLimit: number,
  memoryLimit: number,
  score: number
}

interface AnswerPoint {
  answerFile: string,
  score: number
}

interface ComparingOptions {
  ignoreLineTailWhiteSpaces: boolean,
  ignoreTextTailLineFeeds: boolean
}

interface ProblemConfig {
  specialJudge: string,
  inputFileName: string,
  outputFileName: string,
  submitFileName: string,
  extraFiles: string[],
  points: DataPoint[],
  answer: AnswerPoint,
  comparingOptions: ComparingOptions,
  useStdIO: boolean,
  compileArgs: string,
  languages: string,
  codeSizeLimit: number
}

interface ProblemEditModel extends ResultModel {
  id: number,
  name: string,
  level: number,
  description: string,
  type: number,
  hidden: boolean,
  config: ProblemConfig
}

export default class ProblemEdit extends React.Component<ProblemEditProps, ProblemEditState> {
  constructor(props: ProblemEditProps) {
    super(props);

    this.state = {
      problem: {
        succeeded: false,
        errorCode: 0,
        errorMessage: "",
        config: {
          answer: {
            answerFile: "",
            score: 0
          },
          codeSizeLimit: 0,
          comparingOptions: {
            ignoreLineTailWhiteSpaces: true,
            ignoreTextTailLineFeeds: true
          },
          compileArgs: "",
          extraFiles: [],
          inputFileName: "",
          languages: "",
          outputFileName: "",
          points: [],
          specialJudge: "",
          submitFileName: "",
          useStdIO: true
        },
        description: "",
        hidden: false,
        id: 0,
        level: 1,
        name: "",
        type: 1
      }
    };

    this.fetchConfig = this.fetchConfig.bind(this);
    this.renderPreview = this.renderPreview.bind(this);
  }

  private problemId: number = 0;

  fetchConfig(problemId: number) {
    if (problemId === 0) {
      this.state.problem.succeeded = true;
      this.setState(this.state as ProblemEditState);
      return;
    }

    Get('/problem/edit', { problemId: problemId })
      .then(res => res.json())
      .then(data => {
        let result = data as ProblemEditModel;
        if (result.succeeded) {
          this.setState({
            problem: result
          } as ProblemEditState);
        }
        else {
          this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '题目配置加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目编辑');

    if (this.props.problemId) this.problemId = this.props.problemId;
    else if (this.props.match.params.problemId) this.problemId = parseInt(this.props.match.params.problemId)
    else this.problemId = 0;

    this.fetchConfig(this.problemId);
  }

  private editor: React.RefObject<CodeEditor> = React.createRef<CodeEditor>();

  renderPreview() {
    let editor = this.editor.current;
    if (!editor) return;
    this.state.problem.description = editor.getInstance().getValue();
    this.setState(this.state as ProblemEditState);
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
    if (!this.state.problem.succeeded) return placeHolder;
    
    return <>
      <div style={{ width: '100%', height: '30em' }}>
        <CodeEditor ref={this.editor} language="markdown" onChange={() => this.renderPreview()} defaultValue={this.state.problem.description}></CodeEditor>
      </div>
      <MarkdownViewer content={this.state.problem.description}></MarkdownViewer>
    </>;
  }
}