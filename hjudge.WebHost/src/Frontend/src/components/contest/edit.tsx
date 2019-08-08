import * as React from 'react';
import { ResultModel } from '../../interfaces/resultModel';
import { CommonProps } from '../../interfaces/commonProps';
import { setTitle } from '../../utils/titleHelper';
import { Get, Put, Post } from '../../utils/requestHelper';
import CodeEditor from '../editor/code';
import { Placeholder, Tab, Grid, Form, Rating, Header, Button, Divider, List, Label, Segment, Icon } from 'semantic-ui-react';
import MarkdownViewer from '../viewer/markdown';

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

interface ContestEditModel extends ResultModel {
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

interface ContestEditProps extends CommonProps {
  contestId?: number
}

interface ContestEditState {
  contest: ContestEditModel
}

export default class ContestEdit extends React.Component<ContestEditProps, ContestEditState> {
  constructor(props: ContestEditProps) {
    super(props);

    this.state = {
      contest: {
        succeeded: false,
        errorCode: 0,
        errorMessage: '',
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

  private contestId: number = 0;
  private editor: React.RefObject<CodeEditor> = React.createRef<CodeEditor>();


  fetchConfig(contestId: number) {
    if (contestId === 0) {
      this.state.contest.succeeded = true;
      this.setState(this.state as ContestEditState);
      return;
    }

    Get('/contest/edit', { contestId: contestId })
      .then(res => res.json())
      .then(data => {
        let result = data as ContestEditModel;
        if (result.succeeded) {
          result.startTime = new Date(result.startTime.toString());
          result.endTime = new Date(result.endTime.toString());
          this.setState({
            contest: result
          } as ContestEditState);
        }
        else {
          this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '比赛配置加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目编辑');

    if (this.props.contestId) this.contestId = this.props.contestId;
    else if (this.props.match.params.contestId) this.contestId = parseInt(this.props.match.params.contestId)
    else this.contestId = 0;

    this.fetchConfig(this.contestId);
  }

  renderPreview() {
    let editor = this.editor.current;
    if (!editor) return;
    this.state.contest.description = editor.getInstance().getValue();
    this.setState(this.state as ContestEditState);
  }

  handleChange(obj: any, name: string, value: any) {
    obj[name] = value;
    this.setState(this.state);
  }

  submitChange() {
    if (!this.canSubmit()) {
      this.props.openPortal('错误', '比赛信息填写不完整', 'red');
      return;
    }
    if (this.state.contest.id === 0) {
      Put('/contest/edit', this.state.contest)
        .then(res => res.json())
        .then(data => {
          let result = data as ContestEditModel;
          if (result.succeeded) {
            result.startTime = new Date(result.startTime.toString());
            result.endTime = new Date(result.endTime.toString());
            this.setState({
              contest: result
            } as ContestEditState);
            this.props.openPortal('成功', '比赛保存成功', 'green');
            this.props.history.replace(`/edit/contest/${result.id}`);
          }
          else {
            this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
          }
        })
        .catch(err => {
          this.props.openPortal('错误', '比赛配置加载失败', 'red');
          console.log(err);
        });
    }
    else {
      Post('/contest/edit', this.state.contest)
        .then(res => res.json())
        .then(data => {
          let result = data as ContestEditModel;
          if (result.succeeded) {
            this.props.openPortal('成功', '比赛保存成功', 'green');
          }
          else {
            this.props.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
          }
        })
        .catch(err => {
          this.props.openPortal('错误', '比赛配置加载失败', 'red');
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
    let placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;
    if (!this.state.contest.succeeded) return placeHolder;

    const basic = <Form>
      <Form.Field error={!this.state.contest.name}>
        <Label>比赛名称</Label>
        <Form.Input required defaultValue={this.state.contest.name} onChange={e => this.handleChange(this.state.contest, 'name', e.target.value)} />
      </Form.Field>
      <Form.Field error={!this.state.contest.startTime.getTime()}>
        <Label>开始时间</Label>
        <Form.Input required type='datetime' defaultValue={this.state.contest.startTime.toLocaleString(undefined, { hour12: false })} onChange={e => this.handleChange(this.state.contest, 'startTime', new Date(e.target.value))} />
      </Form.Field>
      <Form.Field error={!this.state.contest.endTime.getTime()}>
        <Label>结束时间</Label>
        <Form.Input required type='datetime' defaultValue={this.state.contest.endTime.toLocaleString(undefined, { hour12: false })} onChange={e => this.handleChange(this.state.contest, 'endTime', new Date(e.target.value))} />
      </Form.Field>
      <Form.Field>
        <Label>进入密码</Label>
        <Form.Input placeholder='留空代表无需密码' defaultValue={this.state.contest.password} onChange={e => this.handleChange(this.state.contest, 'password', e.target.value)} />
      </Form.Field>
      <Form.Group inline>
        <Label>可见性</Label>
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
        <Label>题目列表</Label>
        <Form.Input placeholder='填写题目编号，多个用英文半角分号 ; 分隔' defaultValue={this.state.contest.problems.length === 0 ? '' : this.state.contest.problems.map(v => v.toString()).reduce((accu, next) => `${accu}; ${next}`)} onChange={e => this.handleChange(this.state.contest, 'problems', e.target.value.split(';').filter(v => !!v.trim()).map(v => parseInt(v.trim())))} />
      </Form.Field>
    </Form>;

    const description = <Grid columns={2} divided>
      <Grid.Row>
        <Grid.Column>
          <div style={{ width: '100%', height: '30em' }}>
            <CodeEditor ref={this.editor} language="markdown" onChange={() => this.renderPreview()} defaultValue={this.state.contest.description}></CodeEditor>
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
        <Label>比赛类型</Label>
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
        <Label>结果反馈</Label>
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
        <Label>结果显示</Label>
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
        <Label>计分模式</Label>
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
        <Label>比赛功能</Label>
        <Form.Checkbox
          label='显示排名'
          checked={this.state.contest.config.showRank}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'showRank', data.checked)}
        />
        <Form.Radio
          label='启用封榜'
          checked={this.state.contest.config.autoStopRank}
          onChange={(_, data) => this.handleChange(this.state.contest.config, 'autoStopRank', data.checked)}
        />
      </Form.Group>
      <Form.Group inline>
        <Label>用户功能</Label>
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
        <Label>提交次数限制（0 代表不限）</Label>
        <Form.Input type='number' min={0} defaultValue={this.state.contest.config.submissionLimit} onChange={e => this.handleChange(this.state.contest.config, 'submissionLimit', e.target.valueAsNumber)} />
      </Form.Field>
      <Form.Field>
        <Label>提交语言限制</Label>
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