import * as React from 'reactn';
import { Item, Placeholder, Popup, Dropdown, Label, Header, Button, Rating, Icon, Tab } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { NavLink } from 'react-router-dom';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import MarkdownViewer from '../viewer/markdown';
import { GlobalState } from '../../interfaces/globalState';
import { tryJson } from '../../utils/responseHelper';
import { getRating } from '../../utils/ratingHelper';
import { SourceModel } from '../result/result';

interface ProblemDetailsProps {
  problemId?: number,
  contestId?: number,
  groupId?: number
}

interface ProblemDetailsState {
  problem: ProblemModel,
  languageChoice: number,
  languageValue: string,
  loaded: boolean,
  submitting: boolean,
  contents: string[]
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
  downvote: number,
  myVote: number,
  sources: string[]
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

interface CodeEditorInstance {
  fileName: string,
  editor: React.RefObject<any>
}

export default class ProblemDetails extends React.Component<ProblemDetailsProps & CommonProps, ProblemDetailsState, GlobalState> {
  constructor() {
    super();
    this.fetchDetail = this.fetchDetail.bind(this);
    this.renderProblemInfo = this.renderProblemInfo.bind(this);
    this.editProblem = this.editProblem.bind(this);
    this.submit = this.submit.bind(this);
    this.gotoStatistics = this.gotoStatistics.bind(this);
    this.voteProblem = this.voteProblem.bind(this);
    this.updateContent = this.updateContent.bind(this);

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
        userName: "",
        myVote: 0,
        sources: []
      },
      languageChoice: 0,
      languageValue: 'plain_text',
      loaded: false,
      submitting: false,
      contents: []
    };
  }

  private editors: CodeEditorInstance[] = [];
  private problemId = 0;
  private contestId = 0;
  private groupId = 0;
  private languageOptions: LanguageOptions[] = [];
  private fillSubmitContents: SourceModel[] = [];

  fetchDetail(problemId: number, contestId: number, groupId: number) {
    Post('/problem/details', {
      problemId: problemId,
      contestId: contestId,
      groupId: groupId
    })
      .then(tryJson)
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

        let editors: CodeEditorInstance[] = [];
        let contents: string[] = [];

        for (let fileName of result.sources) {
          editors.push({
            editor: React.createRef<any>(),
            fileName: fileName
          });
          let content = '';
          if (this.fillSubmitContents.length !== 0) {
            let source = this.fillSubmitContents.find(v => v.fileName === fileName);
            if (source) {
              content = source.content;
            }
          }
          contents.push(content);
        }
        this.editors = editors;
        this.setState({
          problem: result,
          languageValue: lang,
          loaded: true,
          contents: contents
        });
        setTitle(result.name);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '题目信息加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目详情');

    if (typeof window !== 'undefined') {
      let fillContent = sessionStorage.getItem('fill-submit');
      if (!!fillContent) {
        sessionStorage.removeItem('fill-submit');
        this.fillSubmitContents = JSON.parse(fillContent);
      }
    }
    
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
    let rating = getRating(this.state.problem.upvote, this.state.problem.downvote);
    return <div>
      <small>添加时间：{this.state.problem.creationTime.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>出题用户：<NavLink to={`/user/${this.state.problem.userId}`}>{this.state.problem.userName}</NavLink></small>
      <br />
      <small>题目难度：<Rating icon='star' rating={this.state.problem.level} maxRating={10} disabled={true} /></small>
      <br />
      <small>完成状态：{this.state.problem.status === 0 ? '未尝试' : this.state.problem.status === 1 ? '已尝试' : '已通过'}</small>
      <br />
      <small>提交统计：{this.state.problem.acceptCount} / {this.state.problem.submissionCount}，通过率：{this.state.problem.submissionCount === 0 ? 0 : Math.round(this.state.problem.acceptCount * 10000 / this.state.problem.submissionCount) / 100.0} %</small>
      <br />
      <small>题目评分：<Popup content={rating.toString()} trigger={<Rating icon='star' rating={Math.round(rating)} maxRating={5} disabled={true} />} /></small>
    </div>;
  }

  editProblem(id: number) {
    this.props.history.push(`/edit/problem/${id}`);
  }

  updateContent(index: number, editor: React.RefObject<any>) {
    if (editor.current) {
      let contents = [...this.state.contents];
      contents[index] = editor.current.editor.getValue()
      this.setState({
        contents: contents
      })
    }
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
    this.setState({ submitting: true });
    Post('/judge/submit', {
      problemId: this.problemId,
      contestId: this.contestId,
      groupId: this.groupId,
      content: this.editors.map((v, i) => ({ fileName: v.fileName, content: this.state.contents[i] })),
      language: this.languageOptions[this.state.languageChoice].text
    })
      .then(tryJson)
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

  gotoStatistics() {
    this.props.history.push(`/statistics/-1/${this.groupId === 0 ? '-1' : this.groupId}/${this.contestId === 0 ? '-1' : this.contestId}/${this.problemId === 0 ? '-1' : this.problemId}/All`);
  }

  voteProblem(voteType: number) {
    if (this.state.problem.myVote !== 0) {
      Post('/vote/cancel', {
        problemId: this.state.problem.id
      })
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          this.global.commonFuncs.openPortal('成功', '评价取消成功', 'green');
          let problem = { ...this.state.problem };
          problem.myVote = 0;
          this.setState({ problem: problem });
        })
        .catch(err => {
          this.global.commonFuncs.openPortal('错误', '评价取消失败', 'red');
          console.log(err);
        })
      return;
    }

    Post('/vote/problem', {
      problemId: this.state.problem.id,
      voteType: voteType
    })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        this.global.commonFuncs.openPortal('成功', '评价成功', 'green');
        let problem = { ...this.state.problem };
        problem.myVote = voteType;
        this.setState({ problem: problem });
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '评价失败', 'red');
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

    this.languageOptions = this.state.problem.languages.map((v, i) => ({
      key: i,
      value: i,
      text: v.name,
      information: v.information,
      highlight: v.syntaxHighlight
    } as LanguageOptions));
    const { languageChoice } = this.state;

    const AceEditor = require('react-ace').default;
    if (typeof window !== 'undefined' && window) {
      let windowAsAny = window as any;
      windowAsAny.ace.config.set('basePath', '/lib/ace');
    }

    const panes = this.editors.map((v, i) => {
      let fileName = v.fileName.replace(/\${.*?}/g, '');
      return {
        menuItem: `${i + 1}. ${!!fileName ? fileName : 'default'}`,
        render: () => <Tab.Pane attached={false} key={i}>
          <div style={{ width: '100%', height: '30em' }}>
            <AceEditor height="100%" width="100%" onChange={() => this.updateContent(i, this.editors[i].editor)} debounceChangePeriod={200} value={this.state.contents[i]} ref={this.editors[i].editor} mode={this.state.languageValue} theme="tomorrow"></AceEditor>
          </div>
        </Tab.Pane>
      }
    });

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
      <Tab menu={{ fluid: true, vertical: true, pointing: true, secondary: true }} panes={panes} menuPosition='right' />
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
                <Button icon onClick={() => this.voteProblem(1)}>
                  <Icon name='thumbs up' color={this.state.problem.myVote === 1 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button icon onClick={() => this.voteProblem(2)}>
                  <Icon name='thumbs down' color={this.state.problem.myVote === 2 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button onClick={this.gotoStatistics}>状态</Button>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button primary onClick={() => this.editProblem(this.state.problem.id)}>编辑</Button> : null}
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%' }}>
              <MarkdownViewer content={this.state.problem.description} />
            </div>
          </Item.Description>
        </Item.Content>
      </Item>
      <Header as='h2'>提交</Header>
      {submitComponent}
    </>;
  }
}