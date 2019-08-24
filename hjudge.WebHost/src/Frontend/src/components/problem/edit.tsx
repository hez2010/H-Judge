import * as React from 'reactn';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { Get, Put, Post, Delete } from '../../utils/requestHelper';
import CodeEditor from '../editor/code';
import { Placeholder, Tab, Grid, Form, Rating, Header, Button, Divider, List, Label, Segment, Icon, Confirm, Loader } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';
import { GlobalState } from '../../interfaces/globalState';
import { tryJson } from '../../utils/responseHelper';

interface ProblemEditState {
  problem: ProblemEditModel,
  useSpecialJudge: boolean,
  selectedTemplate: string,
  processingData: boolean,
  confirmOpen: boolean,
  loaded: boolean
}

interface ProblemDataUploadModel {
  failedFiles: string[]
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

interface ProblemEditModel {
  id: number,
  name: string,
  level: number,
  description: string,
  type: number,
  hidden: boolean,
  config: ProblemConfig
}

export default class ProblemEdit extends React.Component<ProblemEditProps, ProblemEditState, GlobalState> {
  constructor() {
    super();

    this.state = {
      problem: {
        config: {
          answer: {
            answerFile: '',
            score: 0
          },
          codeSizeLimit: 0,
          comparingOptions: {
            ignoreLineTailWhiteSpaces: true,
            ignoreTextTailLineFeeds: true
          },
          compileArgs: '',
          extraFiles: [],
          inputFileName: '',
          languages: '',
          outputFileName: '',
          points: [],
          specialJudge: '',
          submitFileName: '',
          useStdIO: true
        },
        description: '',
        hidden: false,
        id: 0,
        level: 1,
        name: '',
        type: 1
      },
      useSpecialJudge: false,
      selectedTemplate: '',
      processingData: false,
      confirmOpen: false,
      loaded: false
    };

    this.fetchConfig = this.fetchConfig.bind(this);
    this.renderPreview = this.renderPreview.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.submitChange = this.submitChange.bind(this);
    this.canSubmit = this.canSubmit.bind(this);
    this.removePoint = this.removePoint.bind(this);
    this.addPoint = this.addPoint.bind(this);
    this.applyTemplate = this.applyTemplate.bind(this);
    this.uploadFile = this.uploadFile.bind(this);
    this.selectFile = this.selectFile.bind(this);
    this.deleteFile = this.deleteFile.bind(this);
    this.downloadFile = this.downloadFile.bind(this);
  }

  private problemId = 0;
  private pointsCount = 0;
  private editor = React.createRef<CodeEditor>();
  private fileLoader = React.createRef<HTMLInputElement>();

  fetchConfig(problemId: number) {
    if (problemId === 0) {
      let state = { ...this.state };
      state.loaded = true;
      this.setState(state);
      return;
    }

    Get('/problem/edit', { problemId: problemId })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemEditModel;
        result.config.points = result.config.points.map(v => { v.index = ++this.pointsCount; return v; });
        this.setState({
          problem: result,
          useSpecialJudge: !!result.config.specialJudge,
          loaded: true
        } as ProblemEditState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目配置加载失败', 'red');
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
      this.global.commonFuncs.openPortal('错误', '题目信息填写不完整', 'red');
      return;
    }
    if (this.state.problem.id === 0) {
      Put('/problem/edit', this.state.problem)
        .then(res => tryJson(res))
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          let result = data as ProblemEditModel;
          result.config.points = result.config.points.map(v => { v.index = ++this.pointsCount; return v; });
          this.setState({
            problem: result,
            useSpecialJudge: !!result.config.specialJudge
          } as ProblemEditState);
          this.global.commonFuncs.openPortal('成功', '题目保存成功', 'green');
          this.props.history.replace(`/edit/problem/${result.id}`);
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '题目保存失败', 'red');
          console.log(err);
        });
    }
    else {
      Post('/problem/edit', this.state.problem)
        .then(res => tryJson(res))
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          this.global.commonFuncs.openPortal('成功', '题目保存成功', 'green');
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '题目配置加载失败', 'red');
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
      this.global.commonFuncs.openPortal('错误', '快速套用模板格式错误', 'red');
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

  uploadFile() {
    let ele = this.fileLoader.current;
    if (!ele || !ele.files || ele.files.length === 0) return;
    let file = ele.files[0];
    if (file.type !== 'application/x-zip-compressed' && file.type !== 'application/zip') {
      this.global.commonFuncs.openPortal('错误', '文件格式不正确', 'red');
      ele.value = '';
      return;
    }
    if (file.size > 134217728) {
      this.global.commonFuncs.openPortal('错误', '文件大小不能超过 128 Mb', 'red');
      ele.value = '';
      return;
    }
    let form = new FormData();
    form.append('problemId', this.state.problem.id.toString());
    form.append('file', file);
    this.setState({ processingData: true });
    Put('/problem/data', form, false, '')
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        else {
          let result = data as ProblemDataUploadModel;
          if (result.failedFiles && result.failedFiles.length !== 0)
            this.global.commonFuncs.openPortal('警告', `部分题目数据上传失败：\n${result.failedFiles.reduce((accu, next) => accu + '\n' + next)}`, 'orange');
          else this.global.commonFuncs.openPortal('成功', '题目数据上传成功', 'green');
        }
        this.setState({ processingData: false });
        let ele = this.fileLoader.current;
        if (ele) ele.value = '';
      })
      .catch(() => {
        this.global.commonFuncs.openPortal('错误', '题目数据上传失败', 'red');
        this.setState({ processingData: false });
        let ele = this.fileLoader.current;
        if (ele) ele.value = '';
      });
  }

  selectFile() {
    if (this.fileLoader.current) {
      this.fileLoader.current.click();
    }
  }

  downloadFile() {
    let link = document.createElement('a');
    link.href = `/problem/data?problemId=${this.state.problem.id}`;
    link.target = '_blank';
    link.click();
    link.remove();
  }

  deleteFile() {
    this.setState({ confirmOpen: false, processingData: true });
    Delete('/problem/data', { problemId: this.state.problem.id })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
        else this.global.commonFuncs.openPortal('成功', '题目数据删除成功', 'green');
        this.setState({ processingData: false });
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目数据删除失败', 'red');
        this.setState({ processingData: false });
        console.log(err);
      })
  }

  render() {
    const placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;
    if (!this.state.loaded) return placeHolder;

    const basic = <Form>
      <Form.Field required error={!this.state.problem.name}>
        <label>题目名称</label>
        <Form.Input required defaultValue={this.state.problem.name} onChange={e => this.handleChange(this.state.problem, 'name', e.target.value)} />
      </Form.Field>
      <Form.Field>
        <label>题目难度</label>
        <Rating icon='star' defaultRating={this.state.problem.level} maxRating={10} onRate={(_, data) => this.handleChange(this.state.problem, 'level', data.rating)} />
      </Form.Field>
      <Form.Group inline>
        <label>题目类型</label>
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
        <label>可见性</label>
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
        <label>输入输出类型</label>
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
          <Form.Group required inline widths='equal'>
            <Form.Field error={!this.state.problem.config.inputFileName}>
              <label>输入文件名</label>
              <Form.Input fluid required defaultValue={this.state.problem.config.inputFileName} onChange={e => this.handleChange(this.state.problem.config, 'inputFileName', e.target.value)} />
            </Form.Field>
            <Form.Field error={!this.state.problem.config.outputFileName}>
              <label>输出文件名</label>
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
                    <Form.Field required error={!v.stdInFile}>
                      <label>输入文件</label>
                      <Form.Input fluid required defaultValue={v.stdInFile} onChange={e => this.handleChange(v, 'stdInFile', e.target.value)} />
                    </Form.Field>
                    <Form.Field required error={!v.stdOutFile}>
                      <label>输出文件</label>
                      <Form.Input fluid required defaultValue={v.stdOutFile} onChange={e => this.handleChange(v, 'stdOutFile', e.target.value)} />
                    </Form.Field>
                  </Form.Group>

                  <Form.Group inline widths='equal'>
                    <Form.Field required>
                      <label>时间限制</label>
                      <Form.Input fluid required min={0} placeholder='单位：ms' type='number' defaultValue={v.timeLimit.toString()} onChange={e => this.handleChange(v, 'timeLimit', e.target.valueAsNumber)} />
                    </Form.Field>
                    <Form.Field required>
                      <label>内存限制</label>
                      <Form.Input fluid required min={0} placeholder='单位：kb' type='number' defaultValue={v.memoryLimit.toString()} onChange={e => this.handleChange(v, 'memoryLimit', e.target.valueAsNumber)} />
                    </Form.Field>
                    <Form.Field required>
                      <label>该点得分</label>
                      <Form.Input fluid required min={0} type='number' defaultValue={v.score.toString()} onChange={e => this.handleChange(v, 'score', e.target.valueAsNumber)} />
                    </Form.Field>
                  </Form.Group>
                </Segment>
              </List.Content>
            </List.Item>
          )}
        </List></> :
        <>
          <Form.Field required error={!this.state.problem.config.answer.answerFile}>
            <label>答案文件</label>
            <Form.Input required defaultValue={this.state.problem.config.answer.answerFile} onChange={e => this.handleChange(this.state.problem.config.answer, 'answerFile', e.target.value)} />
          </Form.Field>
          <Form.Field required>
            <label>该题得分</label>
            <Form.Input required min={0} type='number' defaultValue={this.state.problem.config.answer.score.toString()} onChange={e => this.handleChange(this.state.problem.config.answer, 'score', e.target.valueAsNumber)} />
          </Form.Field>
        </>
      }
    </Form>;

    const advanced = <Form>
      <Form.Group inline>
        <label>比较程序</label>
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
          <Form.Field required error={!this.state.problem.config.specialJudge}>
            <label>自定义比较程序</label>
            <Form.Input required defaultValue={this.state.problem.config.specialJudge} onChange={e => this.handleChange(this.state.problem.config, 'specialJudge', e.target.value)} />
          </Form.Field> :
          <Form.Group inline>
            <label>比较选项</label>
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
        <label>自定义提交文件名</label>
        <Form.Input placeholder='留空保持默认' defaultValue={this.state.problem.config.submitFileName} onChange={e => this.handleChange(this.state.problem.config, 'submitFileName', e.target.value)} />
      </Form.Field>
      {
        this.state.problem.type === 1 ?
          <>
            <Form.Field>
              <label>提交语言限制</label>
              <Form.Input placeholder='多个用英文半角分号 ; 分隔，留空为不限' defaultValue={this.state.problem.config.languages} onChange={e => this.handleChange(this.state.problem.config, 'languages', e.target.value)} />
            </Form.Field>
            <Form.Field>
              <label>自定义编译参数</label>
              <Form.TextArea placeholder='一行一个，格式：[语言]参数' defaultValue={this.state.problem.config.compileArgs} onChange={(_, data) => this.handleChange(this.state.problem.config, 'compileArgs', data.value)} />
            </Form.Field>
          </> : null
      }
      <Form.Field>
        <label>附加文件</label>
        <Form.TextArea placeholder='一行一个' defaultValue={this.state.problem.config.extraFiles.length === 0 ? '' : this.state.problem.config.extraFiles.reduce((accu, next) => `${accu}\n${next}`)} onChange={(_, data) => this.handleChange(this.state.problem.config, 'extraFiles', data.value ? data.value.toString().split('\n') : [])} />
      </Form.Field>
      <Form.Field>
        <label>提交长度限制（单位：byte，0 为不限）</label>
        <Form.Input type='number' defaultValue={this.state.problem.config.codeSizeLimit} onChange={(_, data) => this.handleChange(this.state.problem.config, 'codeSizeLimit', data.value)} />
      </Form.Field>
    </Form>;

    const utils = <Form>
      <input ref={this.fileLoader} onChange={this.uploadFile} type='file' accept="application/x-zip-compressed,application/zip" style={{ filter: 'alpha(opacity=0)', opacity: 0, width: 0, height: 0 }} />
      {
        !this.state.processingData ? <Form.Group inline>
        <Form.Button type='button' primary onClick={this.selectFile}>上传 .zip 数据文件</Form.Button>
        <Form.Button type='button' onClick={this.downloadFile}>下载数据文件</Form.Button>
        <Form.Button type='button' color='red' onClick={() => this.setState({ confirmOpen: true })}>删除数据文件</Form.Button>
      </Form.Group> : <Loader active inline>处理中...</Loader>
      }
    </Form>;

    let panes = [
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
    ];

    if (this.state.problem.id !== 0) panes =
      [
        ...panes,
        {
          menuItem: '实用工具', render: () => <Tab.Pane key={4} attached={false}>{utils}</Tab.Pane>
        }
      ];

    return <>
      <Header as='h2'>题目编辑</Header>
      <Tab menu={{ secondary: true, pointing: true }} panes={panes} />
      <Divider />
      <Button disabled={!this.canSubmit()} primary onClick={this.submitChange}>保存</Button>

      <Confirm
        open={this.state.confirmOpen}
        cancelButton='取消'
        confirmButton='确定'
        onCancel={() => this.setState({ confirmOpen: false })}
        onConfirm={this.deleteFile}
        content={"删除后不可恢复，确定继续？"}
      />
    </>;
  }
}
