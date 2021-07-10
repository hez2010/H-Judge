import * as React from 'react';
import { Item, Placeholder, Popup, Dropdown, Label, Header, Button, Rating, Icon, Tab } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { setTitle } from '../../utils/titleHelper';
import { NavLink } from 'react-router-dom';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import MarkdownViewer from '../viewer/markdown';
import { tryJson } from '../../utils/responseHelper';
import { getRating } from '../../utils/ratingHelper';
import { SourceModel } from '../result/result';
import AppContext from '../../AppContext';

interface ProblemDetailsProps {
  problemId?: number,
  contestId?: number,
  groupId?: number
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

const ProblemDetails = (props: ProblemDetailsProps & CommonProps) => {
  const [problem, setProblem] = React.useState<ProblemModel>({
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
  });

  const [languageChoice, setLanguageChoice] = React.useState<number>(0);
  const [languageValue, setLanguageValue] = React.useState<string>("plain_text");
  const [loaded, setLoaded] = React.useState<boolean>(false);
  const [submitting, setSubmitting] = React.useState<boolean>(false);
  const [contents, setContents] = React.useState<string[]>([]);

  const editors = React.useRef<CodeEditorInstance[]>([]);
  const problemId = React.useRef(0);
  const contestId = React.useRef(0);
  const groupId = React.useRef(0);
  const languageOptions = React.useRef<LanguageOptions[]>([]);
  const fillSubmitContents = React.useRef<SourceModel[]>([]);

  const { userInfo, commonFuncs } = React.useContext(AppContext);

  const fetchDetail = (problemId: number, contestId: number, groupId: number) => {
    Post('/problem/details', {
      problemId: problemId,
      contestId: contestId,
      groupId: groupId
    })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemModel;
        result.creationTime = new Date(result.creationTime.toString());
        let lang = 'plain_text';
        if (result.languages && result.languages.length > 0) lang = result.languages[0].syntaxHighlight;

        let editorsTmp: CodeEditorInstance[] = [];
        let contents: string[] = [];

        for (let fileName of result.sources) {
          editorsTmp.push({
            editor: React.createRef<any>(),
            fileName: fileName
          });
          let content = '';
          if (fillSubmitContents.current.length !== 0) {
            let source = fillSubmitContents.current.find(v => v.fileName === fileName);
            if (source) {
              content = source.content;
            }
          }
          contents.push(content);
        }

        editors.current = editorsTmp;
        setProblem(result);
        setLanguageValue(lang);
        setLoaded(true);
        setContents(contents);
        setTitle(result.name);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '题目信息加载失败', 'red');
        console.log(err);
      });
  }

  React.useEffect(() => {
    setTitle('题目详情');

    if (typeof window !== 'undefined') {
      let fillContent = sessionStorage.getItem('fill-submit');
      if (!!fillContent) {
        sessionStorage.removeItem('fill-submit');
        fillSubmitContents.current = JSON.parse(fillContent);
      }
    }

    if (props.problemId) problemId.current = props.problemId;
    else if (props.match.params.problemId) problemId.current = parseInt(props.match.params.problemId)
    else problemId.current = 0;

    if (props.contestId) contestId.current = props.contestId;
    else if (props.match.params.contestId) contestId.current = parseInt(props.match.params.contestId)
    else contestId.current = 0;

    if (props.groupId) groupId.current = props.groupId;
    else if (props.match.params.groupId) groupId.current = parseInt(props.match.params.groupId)
    else groupId.current = 0;

    fetchDetail(problemId.current, contestId.current, groupId.current);
  }, []);


  const renderProblemInfo = () => {
    let rating = getRating(problem.upvote, problem.downvote);
    return <div>
      <small>添加时间：{problem.creationTime.toLocaleString(undefined, { hour12: false })}</small>
      <br />
      <small>出题用户：<NavLink to={`/user/${problem.userId}`}>{problem.userName}</NavLink></small>
      <br />
      <small>题目难度：<Rating icon='star' rating={problem.level} maxRating={10} disabled={true} /></small>
      <br />
      <small>完成状态：{problem.status === 0 ? '未尝试' : problem.status === 1 ? '已尝试' : '已通过'}</small>
      <br />
      <small>提交统计：{problem.acceptCount} / {problem.submissionCount}，通过率：{problem.submissionCount === 0 ? 0 : Math.round(problem.acceptCount * 10000 / problem.submissionCount) / 100.0} %</small>
      <br />
      <small>题目评分：<Popup content={rating.toString()} trigger={<Rating icon='star' rating={Math.round(rating)} maxRating={5} disabled={true} />} /></small>
    </div>;
  }

  const editProblem = (id: number) => {
    props.history.push(`/edit/problem/${id}`);
  }

  const updateContent = (index: number, editor: React.RefObject<any>) => {
    if (editor.current) {
      let contentsTmp = [...contents];
      contentsTmp[index] = editor.current.editor.getValue();
      setContents(contentsTmp);
    }
  }

  const submit = () => {
    if (submitting) {
      commonFuncs?.openPortal('提示', '正在提交中，请稍等', 'orange');
      return;
    }
    if (!languageOptions.current[languageChoice]) {
      commonFuncs?.openPortal('提示', '请选择语言', 'orange');
      return;
    }
    setSubmitting(true);
    Post('/judge/submit', {
      problemId: problemId.current,
      contestId: contestId.current,
      groupId: groupId.current,
      content: editors.current.map((v, i) => ({ fileName: v.fileName, content: contents[i] })),
      language: languageOptions.current[languageChoice].text
    })
      .then(tryJson)
      .then(data => {
        setSubmitting(false);
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        commonFuncs?.openPortal('成功', '提交成功', 'green');

        let result = data as SubmitResultModel;
        if (result.jump) props.history.push(`/result/${result.resultId}`);
      }).catch(err => {
        setSubmitting(false);
        commonFuncs?.openPortal('错误', '提交失败', 'red');
        console.log(err);
      });
  }

  const gotoStatistics = () => {
    props.history.push(`/statistics/-1/${groupId.current === 0 ? '-1' : groupId.current}/${contestId.current === 0 ? '-1' : contestId.current}/${problemId.current === 0 ? '-1' : problemId.current}/All`);
  }

  const voteProblem = (voteType: number) => {
    if (problem.myVote !== 0) {
      Post('/vote/cancel', {
        problemId: problem.id
      })
        .then(tryJson)
        .then(data => {
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          commonFuncs?.openPortal('成功', '评价取消成功', 'green');
          let problemTmp = { ...problem };
          problemTmp.myVote = 0;
          setProblem(problemTmp);
        })
        .catch(err => {
          commonFuncs?.openPortal('错误', '评价取消失败', 'red');
          console.log(err);
        })
      return;
    }

    Post('/vote/problem', {
      problemId: problem.id,
      voteType: voteType
    })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        commonFuncs?.openPortal('成功', '评价成功', 'green');
        let problemTmp = { ...problem };
        problemTmp.myVote = voteType;
        setProblem(problemTmp);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '评价失败', 'red');
        console.log(err);
      })
  }

  const render = () => {
    const placeHolder = <Placeholder>
      <Placeholder.Paragraph>
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
        <Placeholder.Line />
      </Placeholder.Paragraph>
    </Placeholder>;
    if (!loaded) return placeHolder;

    languageOptions.current = problem.languages.map((v, i) => ({
      key: i,
      value: i,
      text: v.name,
      information: v.information,
      highlight: v.syntaxHighlight
    } as LanguageOptions));

    const AceEditor = require('react-ace').default;
    if (typeof window !== 'undefined' && window) {
      let windowAsAny = window as any;
      windowAsAny.ace.config.set('basePath', '/lib/ace');
    }

    const panes = editors.current.map((v, i) => {
      let fileName = v.fileName.replace(/\${.*?}/g, '');
      return {
        menuItem: `${i + 1}. ${!!fileName ? fileName : 'default'}`,
        render: () => <Tab.Pane attached={false} key={i}>
          <div style={{ width: '100%', height: '30em' }}>
            <AceEditor height="100%" width="100%" onChange={() => updateContent(i, editors.current[i].editor)} debounceChangePeriod={200} value={contents[i]} ref={editors.current[i].editor} mode={languageValue} theme="tomorrow"></AceEditor>
          </div>
        </Tab.Pane>
      }
    });

    const submitComponent = userInfo?.signedIn ? <><div>
      <Dropdown
        placeholder='代码语言'
        fluid
        search
        selection
        options={languageOptions.current}
        value={languageChoice}
        onChange={(_, { value }) => {
          let lang = languageOptions.current[value as number] ? languageOptions.current[value as number].highlight : 'plain_text';
          setLanguageChoice(value as number);
          setLanguageValue(lang);
        }}
      />
      <Label pointing>{languageOptions.current[languageChoice] ? languageOptions.current[languageChoice].information : '请选择语言'}</Label>
    </div>
      <br />
      <Tab menu={{ fluid: true, vertical: true, pointing: true, secondary: true }} panes={panes} menuPosition='right' />
      <br />
      <div style={{ textAlign: 'right' }}>
        <Button disabled={submitting} primary onClick={submit}>提交</Button>
      </div></>
      : <p>请先登录账户</p>;

    return <>
      <Item>
        <Item.Content>
          <Item.Header as='h2'>
            <Popup flowing hoverable position='right center' trigger={<span>{problem.name}</span>}>
              <Popup.Header>题目信息</Popup.Header>
              <Popup.Content>{renderProblemInfo()}</Popup.Content>
            </Popup>
            <div style={{ float: 'right' }}>
              <Button.Group>
                <Button icon onClick={() => voteProblem(1)}>
                  <Icon name='thumbs up' color={problem.myVote === 1 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button icon onClick={() => voteProblem(2)}>
                  <Icon name='thumbs down' color={problem.myVote === 2 ? 'orange' : 'black'}></Icon>
                </Button>
                <Button onClick={gotoStatistics}>状态</Button>
                {userInfo?.userId && isTeacher(userInfo.privilege) ? <Button primary onClick={() => editProblem(problem.id)}>编辑</Button> : null}
              </Button.Group>
            </div>
          </Item.Header>

          <Item.Description>
            <div style={{ overflow: 'auto', scrollBehavior: 'auto', width: '100%' }}>
              <MarkdownViewer content={problem.description} />
            </div>
          </Item.Description>
        </Item.Content>
      </Item>
      <Header as='h2'>提交</Header>
      {submitComponent}
    </>;
  }

  return render();
};

export default ProblemDetails;