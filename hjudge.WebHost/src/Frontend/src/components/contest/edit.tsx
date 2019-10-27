import * as React from 'reactn';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { Get, Put, Post } from '../../utils/requestHelper';
import { Placeholder, Tab, Grid, Form, Header, Button, Divider } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';
import { tryJson } from '../../utils/responseHelper';
import { toInputDateTime } from '../../utils/dateHelper';

enum ContestType {
  Generic,
  LastSubmit,
  Penalty
}

enum ResultDisplayMode {
  Intime,
  AfterContest,
  Never
}

enum ResultDisplayType {
  Detailed,
  Summary
}

enum ScoreCountingMode {
  All,
  OnlyAccepted
}

interface ContestConfig {
  type: ContestType,
  submissionLimit: number,
  resultMode: ResultDisplayMode,
  resultType: ResultDisplayType,
  showRank: boolean,
  scoreMode: ScoreCountingMode,
  autoStopRank: boolean,
  languages: string,
  canMakeResultPublic: boolean,
  canDiscussion: boolean
}

interface ContestEditModel {
  id: number,
  name: string,
  startTime: Date,
  endTime: Date,
  hidden: boolean,
  password: string,
  description: string,
  config: ContestConfig,
  problems: number[]
}

interface ContestEditProps {
  contestId?: number
}

interface ContestEditState {
  loaded: boolean,
  contest: ContestEditModel
}

export default class ContestEdit extends React.Component<ContestEditProps & CommonProps, ContestEditState, GlobalState> {
  constructor() {
    super();

    this.state = {
      loaded: false,
      contest: {
        config: {
          type: ContestType.Generic,
          submissionLimit: 0,
          resultMode: ResultDisplayMode.Intime,
          resultType: ResultDisplayType.Detailed,
          showRank: true,
          autoStopRank: false,
          scoreMode: ScoreCountingMode.All,
          languages: '',
          canDiscussion: true,
          canMakeResultPublic: false
        },
        description: '',
        hidden: false,
        id: 0,
        name: '',
        password: '',
        startTime: new Date(Date.now()),
        endTime: new Date(Date.now()),
        problems: []
      }
    };

    this.fetchConfig = this.fetchConfig.bind(this);
    this.renderPreview = this.renderPreview.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.submitChange = this.submitChange.bind(this);
    this.canSubmit = this.canSubmit.bind(this);
  }

  private contestId = 0;
  private editor = React.createRef<any>();


  fetchConfig(contestId: number) {
    if (contestId === 0) {
      this.setState({ loaded: true });
      return;
    }

    Get('/contest/edit', { contestId: contestId })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ContestEditModel;
        result.startTime = new Date(result.startTime.toString());
        result.endTime = new Date(result.endTime.toString());
        this.setState({
          contest: result,
          loaded: true
        } as ContestEditState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '比赛配置加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('比赛编辑');

    if (this.props.contestId) this.contestId = this.props.contestId;
    else if (this.props.match.params.contestId) this.contestId = parseInt(this.props.match.params.contestId)
    else this.contestId = 0;

    this.fetchConfig(this.contestId);

    let global = (globalThis as any);
    if (global.ace) global.ace.config.set('basePath', '/lib/ace');
  }

  renderPreview() {
    let editor = this.editor.current;
    if (!editor) return;
    this.state.contest.description = editor.editor.getValue();
    this.setState(this.state as ContestEditState);
  }

  handleChange(obj: any, name: string, value: any) {
    obj[name] = value;
    this.setState(this.state);
  }

  submitChange() {
    if (!this.canSubmit()) {
      this.global.commonFuncs.openPortal('错误', '比赛信息填写不完整', 'red');
      return;
    }
    if (this.state.contest.id === 0) {
      Put('/contest/edit', this.state.contest)
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          let result = data as ContestEditModel;
          result.startTime = new Date(result.startTime.toString());
          result.endTime = new Date(result.endTime.toString());
          this.setState({
            contest: result
          } as ContestEditState);
          this.global.commonFuncs.openPortal('成功', '比赛保存成功', 'green');
          this.props.history.replace(`/edit/contest/${result.id}`);
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '比赛保存失败', 'red');
          console.log(err);
        });
    }
    else {
      Post('/contest/edit', this.state.contest)
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          this.global.commonFuncs.openPortal('成功', '比赛保存成功', 'green');
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '比赛配置加载失败', 'red');
          console.log(err);
        });
    }
  }

  canSubmit() {
    let { contest } = this.state;

    let result = true;
    result = result && !!contest.name;
    result = result && !!contest.startTime.getTime();
    result = result && !!contest.endTime.getTime();

    return result;
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
      <Form.Field required error={!this.state.contest.name}>
        <label>比赛名称</label>
        <Form.Input required defaultValue={this.state.contest.name} onChange={e => this.handleChange(this.state.contest, 'name', e.target.value)} />
      </Form.Field>
      <Form.Field required error={!this.state.contest.startTime.getTime()}>
        <label>开始时间</label>
        <Form.Input type="datetime-local" step="1" required defaultValue={toInputDateTime(this.state.contest.startTime)} onChange={e => this.handleChange(this.state.contest, 'startTime', new Date(e.target.value))} />
      </Form.Field>
      <Form.Field required error={!this.state.contest.endTime.getTime()}>
        <label>结束时间</label>
        <Form.Input type="datetime-local" step="1" required defaultValue={toInputDateTime(this.state.contest.endTime)} onChange={e => this.handleChange(this.state.contest, 'endTime', new Date(e.target.value))} />
      </Form.Field>
      <Form.Field>
        <label>进入密码</label>
        <Form.Input placeholder='留空代表无需密码' defaultValue={this.state.contest.password} onChange={e => this.handleChange(this.state.contest, 'password', e.target.value)} />
      </Form.Field>
      <Form.Group inline>
        <label>可见性</label>
        <Form.Radio
          label='显示比赛'
          checked={!this.state.contest.hidden}
          onChange={(_, data) => this.handleChange(this.state.contest, 'hidden', !data.checked)}
        />
        <Form.Radio
          label='隐藏比赛'
          checked={this.state.contest.hidden}
          onChange={(_, data) => this.handleChange(this.state.contest, 'hidden', data.checked)}
        />
      </Form.Group>
      <Form.Field>
        <label>题目列表</label>
        <Form.Input placeholder='填写题目编号，多个用英文半角分号 ; 分隔' defaultValue={this.state.contest.problems.length === 0 ? '' : this.state.contest.problems.map(v => v.toString()).reduce((accu, next) => `${accu}; ${next}`)} onChange={e => this.handleChange(this.state.contest, 'problems', e.target.value.split(';').filter(v => !!v.trim()).map(v => parseInt(v.trim())))} />
      </Form.Field>
    </Form>;

    const AceEditor = require('react-ace').default;
    if (typeof window !== 'undefined' && window) {
      let windowAsAny = window as any;
      windowAsAny.ace.config.set('basePath', '/lib/ace');
    }

    const description = <Grid columns={2} divided>
      <Grid.Row>
        <Grid.Column>
          <div style={{ width: '100%', height: '30em' }}>
            <AceEditor height="100%" width="100%" ref={this.editor} mode="markdown" debounceChangePeriod={200} theme="tomorrow" onChange={() => this.renderPreview()} value={this.state.contest.description}></AceEditor>
          </div>
        </Grid.Column>
        <Grid.Column>
          <div style={{ width: '100%', height: '30em', overflow: 'auto', scrollBehavior: 'auto' }}>
            <MarkdownViewer content={this.state.contest.description}></MarkdownViewer>
          </div>
        </Grid.Column>
      </Grid.Row>
    </Grid>;

    const advanced = <Form>
      <Form.Group inline>
        <label>比赛类型</label>
        <Form.Radio
          label='普通计时赛'
          checked={this.state.contest.config.type === ContestType.Generic}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'type', ContestType.Generic) }}
        />
        <Form.Radio
          label='最后提交赛'
          checked={this.state.contest.config.type === ContestType.LastSubmit}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'type', ContestType.LastSubmit) }}
        />
        <Form.Radio
          label='罚时计时赛'
          checked={this.state.contest.config.type === ContestType.Penalty}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'type', ContestType.Penalty) }}
        />
      </Form.Group>
      <Form.Group inline>
        <label>结果反馈</label>
        <Form.Radio
          label='即时反馈'
          checked={this.state.contest.config.resultMode === ResultDisplayMode.Intime}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'resultMode', ResultDisplayMode.Intime) }}
        />
        <Form.Radio
          label='赛后反馈'
          checked={this.state.contest.config.resultMode === ResultDisplayMode.AfterContest}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'resultMode', ResultDisplayMode.AfterContest) }}
        />
        <Form.Radio
          label='不反馈'
          checked={this.state.contest.config.resultMode === ResultDisplayMode.Never}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'resultMode', ResultDisplayMode.Never) }}
        />
      </Form.Group>
      <Form.Group inline>
        <label>结果显示</label>
        <Form.Radio
          label='详细结果'
          checked={this.state.contest.config.resultType === ResultDisplayType.Detailed}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'resultType', ResultDisplayType.Detailed) }}
        />
        <Form.Radio
          label='简略结果'
          checked={this.state.contest.config.resultType === ResultDisplayType.Summary}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'resultType', ResultDisplayType.Summary) }}
        />
      </Form.Group>
      <Form.Group inline>
        <label>计分模式</label>
        <Form.Radio
          label='全部计分'
          checked={this.state.contest.config.scoreMode === ScoreCountingMode.All}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'scoreMode', ScoreCountingMode.All) }}
        />
        <Form.Radio
          label='仅计 Accepted'
          checked={this.state.contest.config.scoreMode === ScoreCountingMode.OnlyAccepted}
          onChange={(_, data) => { if (data.checked) this.handleChange(this.state.contest.config, 'scoreMode', ScoreCountingMode.OnlyAccepted) }}
        />
      </Form.Group>
      <Form.Group inline>
        <label>比赛功能</label>
        <Form.Checkbox
          label='显示排名'
          checked={this.state.contest.config.showRank}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'showRank', data.checked)}
        />
        <Form.Checkbox
          label='启用封榜'
          checked={this.state.contest.config.autoStopRank}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'autoStopRank', data.checked)}
        />
      </Form.Group>
      <Form.Group inline>
        <label>用户功能</label>
        <Form.Checkbox
          label='允许公开代码'
          checked={this.state.contest.config.canMakeResultPublic}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'canMakeResultPublic', data.checked)}
        />
        <Form.Checkbox
          label='允许参与讨论'
          checked={this.state.contest.config.canDiscussion}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'canDiscussion', data.checked)}
        />
      </Form.Group>
      <Form.Field>
        <label>提交次数限制（0 代表不限）</label>
        <Form.Input type='number' min={0} defaultValue={this.state.contest.config.submissionLimit} onChange={e => this.handleChange(this.state.contest.config, 'submissionLimit', e.target.valueAsNumber)} />
      </Form.Field>
      <Form.Field>
        <label>提交语言限制</label>
        <Form.Input placeholder='多个用英文半角分号 ; 分隔，留空为不限' defaultValue={this.state.contest.config.languages} onChange={e => this.handleChange(this.state.contest.config, 'languages', e.target.value)} />
      </Form.Field>
    </Form>;

    const panes = [
      {
        menuItem: '基本信息', render: () => <Tab.Pane key={0} attached={false}>{basic}</Tab.Pane>
      },
      {
        menuItem: '比赛描述', render: () => <Tab.Pane key={1} attached={false}>{description}</Tab.Pane>
      },
      {
        menuItem: '高级选项', render: () => <Tab.Pane key={3} attached={false}>{advanced}</Tab.Pane>
      }
    ]

    return <>
      <Header as='h2'>比赛编辑</Header>
      <Tab menu={{ secondary: true, pointing: true }} panes={panes} />
      <Divider />
      <Button disabled={!this.canSubmit()} primary onClick={this.submitChange}>保存</Button>
    </>;
  }
}