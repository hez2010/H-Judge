import * as React from 'react';
import { match } from 'react-router';
import { History, Location } from 'history';
import { SemanticCOLORS, Item, Placeholder, Portal, Segment, Header, Card, Popup } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { setTitle } from '../../utils/titleHelper';
import { NavLink } from 'react-router-dom';
import hljs from 'highlight.js';

interface ProblemDetailsProps {
  match: match<any>,
  history: History<any>,
  location: Location<any>,
  openPortal: ((header: string, message: string, color: SemanticCOLORS) => void)
}

interface ProblemDetailsState {
  problem: ProblemModel
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
  languages: string[],
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
      }
    };
  }

  fetchDetail(problemId: number, contestId: number, groupId: number) {
    Post('/Problem/ProblemDetail', {
      problemId: problemId,
      contestId: contestId,
      groupId: groupId
    })
      .then(res => res.json())
      .then(data => {
        let result = data as ProblemModel;
        if (result.succeeded) {
          result.creationTime = new Date(result.creationTime.toString());
          this.setState({
            problem: result
          } as ProblemDetailsState);
          setTitle(result.name);
        }
        else {
          this.props.openPortal('错误', `题目信息加载失败\n${result.errorMessage} (${result.errorCode})`, 'red');
        }
      })
      .catch(err => {
        this.props.openPortal('错误', '题目信息加载失败', 'red');
        console.log(err);
      });
  }

  componentDidMount() {
    setTitle('题目详情');
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

  render() {
    let placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;

    return !this.state.problem.succeeded ? placeHolder : <>
      <Item>
        <Item.Content>
          <Item.Header as='h2'>
            <Popup flowing hoverable position='right center' trigger={<span>{this.state.problem.name}</span>}>
              <Popup.Header>题目信息</Popup.Header>
              <Popup.Content>{this.renderProblemInfo()}</Popup.Content>
            </Popup>
          </Item.Header>

          <Item.Description>
            <div dangerouslySetInnerHTML={{ __html: this.state.problem.description }}></div>
          </Item.Description>
          <Item.Extra>

          </Item.Extra>
        </Item.Content>
      </Item>
    </>;
  }
}