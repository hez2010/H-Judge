import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Button, Pagination, Table, Form, Input, Select, Placeholder, Rating, Confirm, Popup } from 'semantic-ui-react';
import { Post, Delete } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import { tryJson } from '../../utils/responseHelper';
import { getRating } from '../../utils/ratingHelper';
import AppContext from '../../AppContext';

interface ProblemProps {
  contestId?: number,
  groupId?: number
}

export interface ProblemListItemModel {
  id: number,
  name: string,
  level: number,
  hidden: boolean,
  status: number,
  acceptCount: number,
  submissionCount: number,
  upvote: number,
  downvote: number
}

interface ProblemListModel {
  problems: ProblemListItemModel[],
  totalCount: number
}

const Problem = (props: ProblemProps & CommonProps) => {
  const [problemList, setProblemList] = React.useState<ProblemListModel>({ problems: [], totalCount: 0 });
  const [statusFilter, setStatusFilter] = React.useState([0, 1, 2]);
  const [deleteItem, setDeleteItem] = React.useState(0);
  const [page, setPage] = React.useState(0);
  const [loaded, setLoaded] = React.useState(false);
  const { userInfo, commonFuncs } = React.useContext(AppContext);
  const idRecord = React.useRef(new Map<number, number>());
  // debounce
  const disableNavi = React.useRef(false);
  const userId = React.useRef(userInfo!.userId);
  const firstRender = React.useRef(true);

  const fetchProblemList = (requireTotalCount: boolean, page: number) => {
    if (!props.contestId && page.toString() !== props.match.params.page)
      props.history.replace(`/problem/${page}`);
    let form = document.querySelector('#filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    req.contestId = props.contestId;
    req.groupId = props.groupId;
    if (idRecord.current.has(page)) req.startId = idRecord.current.get(page)! + 1;

    Post('/problem/list', req)
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as ProblemListModel;
        let countBackup = problemList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;

        if (result.problems.length > 0)
          idRecord.current.set(page + 1, result.problems[result.problems.length - 1].id);

        setProblemList(result);
        setPage(page);
        setLoaded(true);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '题目列表加载失败', 'red');
        console.log(err);
      })
  }

  const gotoDetails = (index: number) => {
    if (disableNavi.current) {
      disableNavi.current = false;
      return;
    }
    if (!props.contestId) props.history.push(`/details/problem/${index}`);
    else if (!props.groupId) props.history.push(`/details/problem/${index}/${props.contestId}`);
    else props.history.push(`/details/problem/${index}/${props.contestId}/${props.groupId}`);
  }

  const editProblem = (id: number) => {
    disableNavi.current = true;
    props.history.push(`/edit/problem/${id}`);
  }

  const deleteProblem = (id: number) => {
    setDeleteItem(0);
    Delete('/problem/edit', { problemId: id })
      .then(tryJson)
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          commonFuncs?.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        commonFuncs?.openPortal('成功', '题目删除成功', 'green');
        fetchProblemList(true, page);
      })
      .catch(err => {
        commonFuncs?.openPortal('错误', '题目删除失败', 'red');
        console.log(err);
      })
  }

  const renderProblemList = () => {
    const renderRating = (upvote: number, downvote: number) => {
      let rating = getRating(upvote, downvote);
      return <Popup content={rating.toString()} trigger={<Rating icon='star' rating={Math.round(rating)} maxRating={5} disabled={true} />} />;
    }
    return <>
      <Table color='black' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>名称</Table.HeaderCell>
            <Table.HeaderCell>难度</Table.HeaderCell>
            <Table.HeaderCell>状态</Table.HeaderCell>
            <Table.HeaderCell>评分</Table.HeaderCell>
            <Table.HeaderCell>通过量/提交量</Table.HeaderCell>
            <Table.HeaderCell>通过率</Table.HeaderCell>
            {userInfo?.userId && isTeacher(userInfo.privilege) ? <Table.HeaderCell textAlign='center'>操作</Table.HeaderCell> : null}
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            problemList.problems.map((v, i) =>
              <Table.Row key={i} warning={v.hidden} onClick={() => gotoDetails(v.id)} style={{ cursor: 'pointer' }}>
                <Table.Cell>{v.id}</Table.Cell>
                <Table.Cell>{v.name}</Table.Cell>
                <Table.Cell><span role='img' aria-label='level'>⭐</span>×{v.level}</Table.Cell>
                <Table.Cell>{v.status === 0 ? '未尝试' : v.status === 1 ? '已尝试' : '已通过'}</Table.Cell>
                <Table.Cell>{renderRating(v.upvote, v.downvote)}</Table.Cell>
                <Table.Cell>{v.acceptCount}/{v.submissionCount}</Table.Cell>
                <Table.Cell>{v.submissionCount === 0 ? 0 : Math.round(v.acceptCount * 10000 / v.submissionCount) / 100.0} %</Table.Cell>
                {userInfo?.userId && isTeacher(userInfo.privilege) ? <Table.Cell textAlign='center'><Button.Group><Button onClick={() => editProblem(v.id)} color='grey'>编辑</Button><Button onClick={() => { disableNavi.current = true; setDeleteItem(v.id); }} color='red'>删除</Button></Button.Group></Table.Cell> : null}
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
  }

  React.useEffect(() => {
    if (!props.contestId && !props.groupId) setTitle('题库');

    if (!props.match.params.page) fetchProblemList(true, 1);
    else fetchProblemList(true, props.match.params.page);
  }, []);

  React.useEffect(() => {
    if (firstRender.current) {
      firstRender.current = false;
      return;
    }

    if (userInfo && userId.current !== userInfo.userId) {
      userId.current = userInfo.userId;
      idRecord.current.clear();
      if (!props.match.params.page) fetchProblemList(true, 1);
      else fetchProblemList(true, props.match.params.page);
    }
  });

  const placeHolder = <Placeholder>
    <Placeholder.Paragraph>
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
    </Placeholder.Paragraph>
  </Placeholder>;

  return <>
    <Form id='filterForm'>
      <Form.Group widths={'equal'}>
        <Form.Field width={4}>
          <label>题目编号</label>
          <Input fluid name='id' type='number' onChange={() => idRecord.current.clear()}></Input>
        </Form.Field>
        <Form.Field width={8}>
          <label>题目名称</label>
          <Input fluid name='name' onChange={() => idRecord.current.clear()}></Input>
        </Form.Field>
        <Form.Field width={8}>
          <label>题目状态</label>
          <Select onChange={(_event, data) => { setStatusFilter(data.value as number[]); idRecord.current.clear(); }} fluid name='status' multiple defaultValue={[0, 1, 2]} options={[{ text: '未尝试', value: 0 }, { text: '已尝试', value: 1 }, { text: '已通过', value: 2 }]}></Select>
        </Form.Field>
        <Form.Field width={4}>
          <label>题目操作</label>
          <Button.Group fluid>
            <Button type='button' primary onClick={() => fetchProblemList(true, 1)}>筛选</Button>
            {userInfo?.userId && isTeacher(userInfo.privilege) ? <Button type='button' secondary onClick={() => editProblem(0)}>添加</Button> : null}
          </Button.Group>
        </Form.Field>
      </Form.Group>
    </Form>
    {renderProblemList()}
    {loaded ? (problemList.problems.length === 0 ? <p>没有数据</p> : null) : placeHolder}
    <div style={{ textAlign: 'center' }}>
      <Pagination
        activePage={page}
        onPageChange={(_event, data) => fetchProblemList(false, data.activePage as number)}
        size='small'
        siblingRange={2}
        boundaryRange={1}
        totalPages={problemList.totalCount === 0 ? 0 : Math.floor(problemList.totalCount / 10) + (problemList.totalCount % 10 === 0 ? 0 : 1)}
        firstItem={null}
        lastItem={null}
      />
    </div>
    <Confirm
      open={deleteItem !== 0}
      cancelButton='取消'
      confirmButton='确定'
      onCancel={() => setDeleteItem(0)}
      onConfirm={() => deleteProblem(deleteItem)}
      content={"删除后不可恢复，确定继续？"}
    />
  </>;
};

export default Problem;