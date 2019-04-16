import * as React from 'react';
import { Item, Placeholder, Popup, Dropdown, Label, Header, Button } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { setTitle } from '../../utils/titleHelper';
import { NavLink } from 'react-router-dom';
import 'highlight.js/styles/github.css';
import 'katex/dist/katex.min.css';
import md from 'markdown-it';
import mk from '../../extensions/markdown-it-math';
import hljs from '../../extensions/markdown-it-code';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import CodeEditor from '../editor/code-editor';

interface ProblemDetailsProps extends CommonProps { }

interface ProblemDetailsState {
  problem: ProblemModel,
  languageChoice: number,
  languageTitle: string
}

interface ProblemModel extends ResultModel {
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

export default class ProblemDetails extends React.Component<ProblemDetailsProps, ProblemDetailsState> {
  constructor(props: ProblemDetailsProps) {
    super(props);

    this.fetchDetail = this.fetchDetail.bind(this);
    this.renderProblemInfo = this.renderProblemInfo.bind(this);
    this.editProblem = this.editProblem.bind(this);

    this.state = {
      problem: {
        acceptCount: 0,
        creationTime: new Date(),
        description: "",
        downvote: 0,
        errorCode: 0,
        errorMessage: "",
        hidden: false,
        id: 0,
        languages: [],
        level: 0,
        name: "",
        status: 0,
        submissionCount: 0,
        succeeded: false,
        type: 0,
        upvote: 0,
        userId: "",
        userName: ""
      },
      languageChoice: 0,
      languageTitle: ''
    };
  }

  private editor: React.Ref<CodeEditor> = React.createRef();

  fetchDetail(problemId: number, contestId: number, groupId: number) {
    Post('/Problem/ProblemDetails', {
      problemId: problemId,
      contestId: contestId,
      groupId: groupId
    })
      .then(res => res.json())
      .then(data => {
        let result = data as ProblemModel;
        if (result.succeeded) {
          result.creationTime = new Date(result.creationTime.toString());
          let lang = 'plain_text';
          if (result.languages && result.languages.length > 0) lang = result.languages[0].syntaxHighlight;
          this.setState({
            problem: result,
            languageTitle: lang
          } as ProblemDetailsState);
          setTitle(result.name);
        }
        else {
          this.props.openPortal('错误', `题目信息加载失败\n${result.errorMessage} (${result.errorCode})`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '题目信息加载失败', 'red'),
          console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目详情'),
      this.fetchDetail(
        this.props.match.params.problemId ? parseInt(this.props.match.params.problemId) : 0,
        this.props.match.params.contestId ? parseInt(this.props.match.params.contestId) : 0,
        this.props.match.params.groupId ? parseInt(this.props.match.params.groupId) : 0,
      );
  }

  renderProblemInfo() {
    return <div>
      <small>添加时间：{this.state.problem.creationTime.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>出题用户：<NavLink to='/'>{this.state.problem.userName}</NavLink></small>
      <br />
      <small>题目难度：{this.state.problem.level}</small>
      <br />
      <small>完成状态：{this.state.problem.status === 0 ? '未尝试' : this.state.problem.status === 1 ? '已尝试' : '已通过'}</small>
      <br />
      <small>提交统计：{this.state.problem.acceptCount} / {this.state.problem.submissionCount}</small>
    </div>;
  }

  editProblem(id: number) {
    this.props.history.push(`/edit/problem/${id}`);
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

    let markdown = new md({ html: true }).use(mk, { throwOnError: false }).use(hljs);

    let langOptions = this.state.problem.languages.map((v, i) => {
      return {
        key: i,
        value: i,
        text: v.name,
        information: v.information,
        highlight: v.syntaxHighlight
      }
    });
    const { languageChoice } = this.state;

    let submitModal = <>
      <Header as='h2'>提交</Header>
      <div>
        <Dropdown
          placeholder='代码语言'
          fluid
          search
          selection
          options={langOptions}
          value={languageChoice}
          onChange={(_, { value }) => {
            let lang = langOptions[value as number] ? langOptions[value as number].highlight : 'plain_text';
            this.setState({ languageChoice: value, languageTitle: lang } as ProblemDetailsState);
          }}
        />
        <Label pointing>{langOptions[languageChoice] ? langOptions[languageChoice].information : '请选择语言'}</Label>
      </div>
      <br />
      <div style={{ width: '100%', height: '30em' }}>
        <CodeEditor ref={this.editor} onChange={(e: any) => console.log(e)} language={this.state.languageTitle}></CodeEditor>
      </div>
      <br />
      <div style={{ textAlign: 'right' }}>
        <Button primary>提交</Button>
      </div>
    </>;

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
                {this.props.userInfo.succeeded && isTeacher(this.props.userInfo.privilege) ? <Button primary onClick={() => this.editProblem(this.state.problem.id)}>编辑</Button> : null}
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            <div dangerouslySetInnerHTML={{ __html: markdown.render(this.state.problem.description) }}></div>
          </Item.Description>
          <Item.Extra>

          </Item.Extra>
        </Item.Content>
      </Item>
      {submitModal}
    </>;
  }
}