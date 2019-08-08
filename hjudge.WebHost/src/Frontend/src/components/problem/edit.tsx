import * as React from 'react';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { Get, Put, Post } from '../../utils/requestHelper';
import CodeEditor from '../editor/code';
import { Placeholder, Tab, Grid, Form, Rating, Header, Button, Divider, List, Label, Segment, Icon } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';

interface ProblemEditState {
  problem: ProblemEditModel,
  useSpecialJudge: boolean,
  selectedTemplate: string
}

interface ProblemEditProps extends CommonProps {
  problemId?: number
}

interface DataPoint {
  stdInFile: string,
  stdOutFile: string,
  timeLimit: number,
  memoryLimit: number,
  score: number,
  index: number
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
      },
      useSpecialJudge: false,
      selectedTemplate: ''
    };

    this.fetchConfig = this.fetchConfig.bind(this);
    this.renderPreview = this.renderPreview.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.submitChange = this.submitChange.bind(this);
    this.canSubmit = this.canSubmit.bind(this);
    this.removePoint = this.removePoint.bind(this);
    this.addPoint = this.addPoint.bind(this);
    this.applyTemplate = this.applyTemplate.bind(this);
  }

  private problemId: number = 0;
  private pointsCount: number = 0;

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
          result.config.points = result.config.points.map(v => { v.index = ++this.pointsCount; return v; });
          this.setState({
            problem: result,
            useSpecialJudge: !!result.config.specialJudge
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

  handleChange(obj: any, name: string, value: any) {
    obj[name] = value;
    this.setState(this.state);
  }

  submitChange() {
    if (!this.canSubmit()) {
      this.props.openPortal('错误', '题目信息填写不完整', 'red');
      return;
    }
    if (this.state.problem.id === 0) {
      Put('/problem/edit', this.state.problem)
        .then(res => res.json())
        .then(data => {
          let result = data as ProblemEditModel;
          if (result.succeeded) {
            result.config.points = result.config.points.map(v => { v.index = ++this.pointsCount; return v; });
            this.setState({
              problem: result,
              useSpecialJudge: !!result.config.specialJudge
            } as ProblemEditState);
            this.props.openPortal('成功', '题目保存成功', 'green');
            this.props.history.replace(`/edit/problem/${result.id}`);
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
    else {
      Post('/problem/edit', this.state.problem)
        .then(res => res.json())
        .then(data => {
          let result = data as ProblemEditModel;
          if (result.succeeded) {
            this.props.openPortal('成功', '题目保存成功', 'green');
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
  }

  canSubmit() {
    let { problem } = this.state;
    let { config } = problem;

    let result = true;
    result = result && !!problem.name;
    if (!config.useStdIO) {
      result = result && !!config.inputFileName;
      result = result && !!config.outputFileName;
    }

    if (problem.type === 1) {
      for (let x in config.points) {
        result = result && !!config.points[x].stdInFile;
        result = result && !!config.points[x].stdOutFile;
      }
      if (this.state.useSpecialJudge) result = result && !!config.specialJudge;
    }
    else {
      result = result && !!config.answer.answerFile;
    }
    return result;
  }

  removePoint = (index: number) => () => {
    this.state.problem.config.points.splice(index, 1);
    this.setState(this.state);
  }

  addPoint() {
    this.state.problem.config.points = [...this.state.problem.config.points, { stdInFile: '', stdOutFile: '', timeLimit: 1000, memoryLimit: 131072, score: 10, index: ++this.pointsCount }];
    this.setState(this.state);
  }

  applyTemplate() {
    let fields = this.state.selectedTemplate.split('|');
    if (fields.length !== 5) {
      this.props.openPortal('错误', '快速套用模板格式错误', 'red');
      return;
    }
    let [input, output, time, memory, score] = fields;
    let points = this.state.problem.config.points;
    for (let i in points) {
      points[i].index = ++this.pointsCount;
      if (input !== '*') points[i].stdInFile = input;
      if (output !== '*') points[i].stdOutFile = output;
      if (time !== '*') points[i].timeLimit = parseInt(time);
      if (memory !== '*') points[i].memoryLimit = parseInt(memory);
      if (score !== '*') points[i].score = parseInt(score);
    }
    this.setState(this.state);
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

    const basic = <Form>
      <Form.Field error={!this.state.problem.name}>
        <Label>题目名称</Label>
        <Form.Input required defaultValue={this.state.problem.name} onChange={e => this.handleChange(this.state.problem, 'name', e.target.value)} />
      </Form.Field>
      <Form.Field>
        <Label>题目难度</Label>
        <Rating icon='star' defaultRating={this.state.problem.level} maxRating={10} onRate={(_, data) => this.handleChange(this.state.problem, 'level', data.rating)} />
      </Form.Field>
      <Form.Group inline>
        <Label>题目类型</Label>
        <Form.Radio
          label='提交代码'
          value={1}
          checked={this.state.problem.type === 1}
          onChange={(_, data) => this.handleChange(this.state.problem, 'type', data.value)}
        />
        <Form.Radio
          label='提交答案'
          value={2}
          checked={this.state.problem.type === 2}
          onChange={(_, data) => this.handleChange(this.state.problem, 'type', data.value)}
        />
      </Form.Group>
      <Form.Group inline>
        <Label>可见性</Label>
        <Form.Radio
          label='显示题目'
          checked={!this.state.problem.hidden}
          onChange={(_, data) => this.handleChange(this.state.problem, 'hidden', !data.checked)}
        />
        <Form.Radio
          label='隐藏题目'
          checked={this.state.problem.hidden}
          onChange={(_, data) => this.handleChange(this.state.problem, 'hidden', data.checked)}
        />
      </Form.Group>
      <Form.Group inline>
        <Label>输入输出类型</Label>
        <Form.Radio
          label='标准输入输出'
          checked={this.state.problem.config.useStdIO}
          onChange={(_, data) => this.handleChange(this.state.problem.config, 'useStdIO', data.checked)}
        />
        <Form.Radio
          label='文件输入输出'
          checked={!this.state.problem.config.useStdIO}
          onChange={(_, data) => this.handleChange(this.state.problem.config, 'useStdIO', !data.checked)}
        />
      </Form.Group>
      {
        this.state.problem.config.useStdIO ? null :
          <Form.Group inline widths='equal'>
            <Form.Field error={!this.state.problem.config.inputFileName}>
              <Label>输入文件名</Label>
              <Form.Input fluid required defaultValue={this.state.problem.config.inputFileName} onChange={e => this.handleChange(this.state.problem.config, 'inputFileName', e.target.value)} />
            </Form.Field>
            <Form.Field error={!this.state.problem.config.outputFileName}>
              <Label>输出文件名</Label>
              <Form.Input fluid required defaultValue={this.state.problem.config.outputFileName} onChange={e => this.handleChange(this.state.problem.config, 'outputFileName', e.target.value)} />
            </Form.Field>
          </Form.Group>
      }
    </Form>;

    const description = <Grid columns={2} divided>
      <Grid.Row>
        <Grid.Column>
          <div style={{ width: '100%', height: '30em' }}>
            <CodeEditor ref={this.editor} language="markdown" onChange={() => this.renderPreview()} defaultValue={this.state.problem.description}></CodeEditor>
          </div>
        </Grid.Column>
        <Grid.Column>
          <div style={{ width: '100%', height: '30em', overflow: 'auto', scrollBehavior: 'auto' }}>
            <MarkdownViewer content={this.state.problem.description}></MarkdownViewer>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>;

    const data = <Form>
      {this.state.problem.type === 1 ? <>
        <Form.Group widths={16} inline>
          <Form.Button width={1} icon='add' primary onClick={this.addPoint} />
          <Form.Input width={14} defaultValue={this.state.selectedTemplate} placeholder='快速套用模板，格式：输入文件|输出文件|时间限制|内存限制|该点得分，* 代表保持不变' list='templates' onChange={e => this.handleChange(this.state, 'selectedTemplate', e.target.value)} />
          <datalist id='templates'>
            <option value='${datadir}/${name}${index}.in|${datadir}/${name}${index}.ans|1000|131072|10' />
            <option value='${datadir}/${name}${index}.in|${datadir}/${name}${index}.out|1000|131072|10' />
            <option value='${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.ans|1000|131072|10' />
            <option value='${datadir}/${name}${index0}.in|${datadir}/${name}${index0}.ans|1000|131072|10' />
          </datalist>
          <Form.Button width={1} icon='checkmark' color='green' onClick={this.applyTemplate} />
        </Form.Group>
        <List relaxed>
          {this.state.problem.config.points.map((v, i) =>
            <List.Item key={v.index}>
              <List.Content>
                <Segment>
                  <Label color='teal' ribbon><span>数据点 #{i + 1}&nbsp;</span><a onClick={this.removePoint(i)}><Icon name='delete' color='red' /></a></Label>

                  <Form.Group inline widths='equal'>
                    <Form.Field error={!v.stdInFile}>
                      <Label>输入文件</Label>
                      <Form.Input fluid required defaultValue={v.stdInFile} onChange={e => this.handleChange(v, 'stdInFile', e.target.value)} />
                    </Form.Field>
                    <Form.Field error={!v.stdOutFile}>
                      <Label>输出文件</Label>
                      <Form.Input fluid required defaultValue={v.stdOutFile} onChange={e => this.handleChange(v, 'stdOutFile', e.target.value)} />
                    </Form.Field>
                  </Form.Group>

                  <Form.Group inline widths='equal'>
                    <Form.Field>
                      <Label>时间限制</Label>
                      <Form.Input fluid required min={0} placeholder='单位：ms' type='number' defaultValue={v.timeLimit.toString()} onChange={e => this.handleChange(v, 'timeLimit', e.target.valueAsNumber)} />
                    </Form.Field>
                    <Form.Field>
                      <Label>内存限制</Label>
                      <Form.Input fluid required min={0} placeholder='单位：kb' type='number' defaultValue={v.memoryLimit.toString()} onChange={e => this.handleChange(v, 'memoryLimit', e.target.valueAsNumber)} />
                    </Form.Field>
                    <Form.Field>
                      <Label>该点得分</Label>
                      <Form.Input fluid required min={0} type='number' defaultValue={v.score.toString()} onChange={e => this.handleChange(v, 'score', e.target.valueAsNumber)} />
                    </Form.Field>
                  </Form.Group>
                </Segment>
              </List.Content>
            </List.Item>
          )}
        </List></> :
        <>
          <Form.Field error={!this.state.problem.config.answer.answerFile}>
            <Label>答案文件</Label>
            <Form.Input required defaultValue={this.state.problem.config.answer.answerFile} onChange={e => this.handleChange(this.state.problem.config.answer, 'answerFile', e.target.value)} />
          </Form.Field>
          <Form.Field>
            <Label>该题得分</Label>
            <Form.Input required min={0} type='number' defaultValue={this.state.problem.config.answer.score.toString()} onChange={e => this.handleChange(this.state.problem.config.answer, 'score', e.target.valueAsNumber)} />
          </Form.Field>
        </>
      }
    </Form>;

    const advanced = <Form>
      <Form.Group inline>
        <Label>比较程序</Label>
        <Form.Radio
          label='默认'
          checked={!this.state.useSpecialJudge}
          onChange={(_, data) => this.handleChange(this.state, 'useSpecialJudge', !data.checked)}
        />
        <Form.Radio
          label='自定义'
          checked={this.state.useSpecialJudge}
          onChange={(_, data) => this.handleChange(this.state, 'useSpecialJudge', data.checked)}
        />
      </Form.Group>
      {
        this.state.useSpecialJudge ?
          <Form.Field error={!this.state.problem.config.specialJudge}>
            <Label>自定义比较程序</Label>
            <Form.Input required defaultValue={this.state.problem.config.specialJudge} onChange={e => this.handleChange(this.state.problem.config, 'specialJudge', e.target.value)} />
          </Form.Field> :
          <Form.Group inline>
            <Label>比较选项</Label>
            <Form.Checkbox
              label='忽略行末空格'
              checked={this.state.problem.config.comparingOptions.ignoreLineTailWhiteSpaces}
              onChange={(_, data) => this.handleChange(this.state.problem.config.comparingOptions, 'ignoreLineTailWhiteSpaces', data.checked)}
            />
            <Form.Checkbox
              label='忽略文末空行'
              checked={this.state.problem.config.comparingOptions.ignoreTextTailLineFeeds}
              onChange={(_, data) => this.handleChange(this.state.problem.config.comparingOptions, 'ignoreTextTailLineFeeds', data.checked)}
            />
          </Form.Group>
      }
      <Form.Field>
        <Label>自定义提交文件名</Label>
        <Form.Input placeholder='留空保持默认' defaultValue={this.state.problem.config.submitFileName} onChange={e => this.handleChange(this.state.problem.config, 'submitFileName', e.target.value)} />
      </Form.Field>
      {
        this.state.problem.type === 1 ?
          <>
            <Form.Field>
              <Label>提交语言限制</Label>
              <Form.Input placeholder='多个用英文半角分号 ; 分隔，留空为不限' defaultValue={this.state.problem.config.languages} onChange={e => this.handleChange(this.state.problem.config, 'languages', e.target.value)} />
            </Form.Field>
            <Form.Field>
              <Label>自定义编译参数</Label>
              <Form.TextArea placeholder='一行一个，格式：[语言]参数' defaultValue={this.state.problem.config.compileArgs} onChange={(_, data) => this.handleChange(this.state.problem.config, 'compileArgs', data.value)} />
            </Form.Field>
          </> : null
      }
      <Form.Field>
        <Label>附加文件</Label>
        <Form.TextArea placeholder='一行一个' defaultValue={this.state.problem.config.extraFiles.length === 0 ? "" : this.state.problem.config.extraFiles.reduce((accu, next) => `${accu}\n${next}`)} onChange={(_, data) => this.handleChange(this.state.problem.config, 'extraFiles', data.value ? data.value.toString().split('\n') : [])} />
      </Form.Field>
      <Form.Field>
        <Label>提交长度限制</Label>
        <Form.Input type='number' placeholder='单位：byte' defaultValue={this.state.problem.config.codeSizeLimit} onChange={(_, data) => this.handleChange(this.state.problem.config, 'codeSizeLimit', data.value)} />
      </Form.Field>
    </Form>;

    const panes = [
      {
        menuItem: '基本信息', render: () => <Tab.Pane key={0} attached={false}>{basic}</Tab.Pane>
      },
      {
        menuItem: '题目描述', render: () => <Tab.Pane key={1} attached={false}>{description}</Tab.Pane>
      },
      {
        menuItem: '题目数据', render: () => <Tab.Pane key={2} attached={false}>{data}</Tab.Pane>
      },
      {
        menuItem: '高级选项', render: () => <Tab.Pane key={3} attached={false}>{advanced}</Tab.Pane>
      }
    ]

    return <>
      <Header as='h2'>题目编辑</Header>
      <Tab menu={{ secondary: true, pointing: true }} panes={panes} />
      <Divider />
      <Button disabled={!this.canSubmit()} primary onClick={this.submitChange}>保存</Button>
    </>;
  }
}
