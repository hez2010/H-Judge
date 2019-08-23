import * as React from 'reactn';
import { Item, Placeholder, Popup, Dropdown, Label, Header, Button, Rating } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { NavLink } from 'react-router-dom';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import CodeEditor from '../editor/code';
import MarkdownViewer from '../viewer/markdown';
import { GlobalState } from '../../interfaces/globalState';
import { tryJson } from '../../utils/responseHelper';

interface ProblemDetailsProps extends CommonProps {
  problemId?: number,
  contestId?: number,
  groupId?: number
}

interface ProblemDetailsState {
  problem: ProblemModel,
  languageChoice: number,
  languageValue: string,
  loaded: boolean,
  submitting: boolean
}

export interface ProblemModel {
  id: number,
  name: string,
  creationTime: Date,
  level: number,
  userId: string,
  userName: string,
  description: string,
  type: number,
  acceptCount: number,
  submissionCount: number,
  languages: { name: string, information: string, syntaxHighlight: string }[],
  status: number,
  hidden: boolean,
  upvote: number,
  downvote: number
}

interface LanguageOptions {
  key: number,
  value: number,
  text: string,
  information: string,
  highlight: string
}

interface SubmitResultModel {
  jump: boolean,
  resultId: number
}

export default class ProblemDetails extends React.Component<ProblemDetailsProps, ProblemDetailsState, GlobalState> {
  constructor() {
    super();
    this.fetchDetail = this.fetchDetail.bind(this);
    this.renderProblemInfo = this.renderProblemInfo.bind(this);
    this.editProblem = this.editProblem.bind(this);
    this.submit = this.submit.bind(this);

    this.state = {
      problem: {
        acceptCount: 0,
        creationTime: new Date(),
        description: "",
        downvote: 0,
        hidden: false,
        id: 0,
        languages: [],
        level: 0,
        name: "",
        status: 0,
        submissionCount: 0,
        type: 0,
        upvote: 0,
        userId: "",
        userName: ""
      },
      languageChoice: 0,
      languageValue: 'plain_text',
      loaded: false,
      submitting: false
    };
  }

  private editor = React.createRef<CodeEditor>();
  private problemId = 0;
  private contestId = 0;
  private groupId = 0;
  private languageOptions: LanguageOptions[] = [];

  fetchDetail(problemId: number, contestId: number, groupId: number) {
    Post('/problem/details', {
      problemId: problemId,
      contestId: contestId,
      groupId: groupId
    })
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemModel;
        result.creationTime = new Date(result.creationTime.toString());
        let lang = 'plain_text';
        if (result.languages && result.languages.length > 0) lang = result.languages[0].syntaxHighlight;
        this.setState({
          problem: result,
          languageValue: lang,
          loaded: true
        } as ProblemDetailsState);
        setTitle(result.name);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目信息加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目详情');

    if (this.props.problemId) this.problemId = this.props.problemId;
    else if (this.props.match.params.problemId) this.problemId = parseInt(this.props.match.params.problemId)
    else this.problemId = 0;

    if (this.props.contestId) this.contestId = this.props.contestId;
    else if (this.props.match.params.contestId) this.contestId = parseInt(this.props.match.params.contestId)
    else this.contestId = 0;

    if (this.props.groupId) this.groupId = this.props.groupId;
    else if (this.props.match.params.groupId) this.groupId = parseInt(this.props.match.params.groupId)
    else this.groupId = 0;

    this.fetchDetail(this.problemId, this.contestId, this.groupId);
  }

  renderProblemInfo() {
    return <div>
      <small>添加时间：{this.state.problem.creationTime.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>出题用户：<NavLink to='/'>{this.state.problem.userName}</NavLink></small>
      <br />
      <small>题目难度：<Rating icon='star' defaultRating={this.state.problem.level} maxRating={10} disabled={true} /></small>
      <br />
      <small>完成状态：{this.state.problem.status === 0 ? '未尝试' : this.state.problem.status === 1 ? '已尝试' : '已通过'}</small>
      <br />
      <small>提交统计：{this.state.problem.acceptCount} / {this.state.problem.submissionCount}</small>
      <br />
      <small>题目评分：{this.state.problem.upvote + this.state.problem.downvote === 0 ? <Rating icon='star' defaultRating={3} maxRating={5} disabled={true} /> : <Rating icon='star' defaultRating={3} maxRating={5} disabled={true} rating={Math.round(this.state.problem.upvote * 5 / (this.state.problem.upvote + this.state.problem.downvote))} />}</small>
    </div>;
  }

  editProblem(id: number) {
    this.props.history.push(`/edit/problem/${id}`);
  }

  submit() {
    if (this.state.submitting) {
      this.global.commonFuncs.openPortal('提示', '正在提交中，请稍等', 'orange');
      return;
    }
    if (!this.languageOptions[this.state.languageChoice]) {
      this.global.commonFuncs.openPortal('提示', '请选择语言', 'orange');
      return;
    }
    let editor = this.editor.current;
    if (!editor) return;
    this.setState({ submitting: true });
    Post('/judge/submit', {
      problemId: this.problemId,
      contestId: this.contestId,
      groupId: this.groupId,
      content: editor.getInstance().getValue(),
      language: this.languageOptions[this.state.languageChoice].text
    })
      .then(res => tryJson(res))
      .then(data => {
        this.setState({ submitting: false });
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('成功', '提交成功', 'green');

        let result = data as SubmitResultModel;
        if (result.jump) this.props.history.push(`/result/${result.resultId}`);
      }).catch(err => {
        this.setState({ submitting: false });
        this.global.commonFuncs.openPortal('错误', '提交失败', 'red');
        console.log(err);
      });
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
    if (!this.state.loaded) return placeHolder;

    this.languageOptions = this.state.problem.languages.map((v, i) => ({
      key: i,
      value: i,
      text: v.name,
      information: v.information,
      highlight: v.syntaxHighlight
    } as LanguageOptions));
    const { languageChoice } = this.state;

    const submitComponent = this.global.userInfo.signedIn ? <><div>
      <Dropdown
        placeholder='代码语言'
        fluid
        search
        selection
        options={this.languageOptions}
        value={languageChoice}
        onChange={(_, { value }) => {
          let lang = this.languageOptions[value as number] ? this.languageOptions[value as number].highlight : 'plain_text';
          this.setState({ languageChoice: value, languageValue: lang } as ProblemDetailsState);
        }}
      />
      <Label pointing>{this.languageOptions[languageChoice] ? this.languageOptions[languageChoice].information : '请选择语言'}</Label>
    </div>
      <br />
      <div style={{ width: '100%', height: '30em' }}>
        <CodeEditor ref={this.editor} language={this.state.languageValue}></CodeEditor>
      </div>
      <br />
      <div style={{ textAlign: 'right' }}>
        <Button disabled={this.state.submitting} primary onClick={this.submit}>提交</Button>
      </div></>
      : <p>请先登录账户</p>;

    return <>
      <Item>
        <Item.Content>
          <Item.Header as='h2'>
            <Popup flowing hoverable position='right center' trigger={<span>{this.state.problem.name}</span>}>
              <Popup.Header>题目信息</Popup.Header>
              <Popup.Content>{this.renderProblemInfo()}</Popup.Content>
            </Popup>
            <div style={{ float: 'right' }}>
              <Button.Group>
                <Button>状态</Button>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button primary onClick={() => this.editProblem(this.state.problem.id)}>编辑</Button> : null}
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%' }}>
              <MarkdownViewer content={this.state.problem.description} />
            </div>
          </Item.Description>
          <Item.Extra>

          </Item.Extra>
        </Item.Content>
      </Item>
      <Header as='h2'>提交</Header>
      {submitComponent}
    </>;
  }
}