import * as React from 'reactn';
import { setTitle } from '../../utils/titleHelper';
import { Button, Pagination, Table, Form, Label, Input, Placeholder, Select } from 'semantic-ui-react';
import { Post } from '../../utils/requestHelper';
import { SerializeForm } from '../../utils/formHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { isTeacher } from '../../utils/privilegeHelper';
import { CommonProps } from '../../interfaces/commonProps';
import { GlobalState } from '../../interfaces/globalState';
import { tryJson } from '../../utils/responseHelper';

interface GroupProps extends CommonProps { }

interface GroupListItemModel {
  id: number,
  name: string,
  userName: string,
  userId: string,
  creationTime: Date,
  isPrivate: boolean
}

interface GroupListModel {
  groups: GroupListItemModel[],
  totalCount: number
}

interface GroupState {
  groupList: GroupListModel,
  statusFilter: number[],
  page: number,
  loaded: boolean
}

export default class Group extends React.Component<GroupProps, GroupState, GlobalState> {
  constructor() {
    super();

    this.renderGroupList = this.renderGroupList.bind(this);
    this.fetchGroupList = this.fetchGroupList.bind(this);
    this.gotoDetails = this.gotoDetails.bind(this);
    this.editGroup = this.editGroup.bind(this);

    this.state = {
      groupList: {
        groups: [],
        totalCount: 0
      },
      statusFilter: [0, 1],
      page: 0,
      loaded: false
    };
  }

  private idRecord = new Map<number, number>();
  private disableNavi = false;
  private userId = this.global.userInfo.userId;

  componentWillUpdate(_nextProps: GroupProps, _nextState: GroupState) {
    if (this.userId !== this.global.userInfo.userId) {
      this.userId = this.global.userInfo.userId;
      this.idRecord.clear();
      if (!this.props.match.params.page) this.fetchGroupList(true, 1);
      else this.fetchGroupList(true, this.props.match.params.page);
    }
  }

  fetchGroupList(requireTotalCount: boolean, page: number) {
    this.props.history.replace(`/group/${page}`);
    let form = document.querySelector('#filterForm') as HTMLFormElement;
    let req: any = {};
    req.filter = SerializeForm(form);
    if (!req.filter.id) req.filter.id = 0;
    req.filter.status = this.state.statusFilter;
    req.start = (page - 1) * 10;
    req.count = 10;
    req.requireTotalCount = requireTotalCount;
    if (this.idRecord.has(page)) req.startId = this.idRecord.get(page)! - 1;

    Post('/group/list', req)
      .then(res => tryJson(res))
      .then(data => {
        let error = data as ErrorModel;
        if (error.errorCode) {
          this.global.commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
          return;
        }
        let result = data as GroupListModel;
        let countBackup = this.state.groupList.totalCount;
        if (!requireTotalCount) result.totalCount = countBackup;
        for (let c in result.groups) {
          result.groups[c].creationTime = new Date(result.groups[c].creationTime.toString());
        }
        if (result.groups.length > 0)
          this.idRecord.set(page + 1, result.groups[result.groups.length - 1].id);
        this.setState({
          groupList: result,
          page: page,
          loaded: true
        } as GroupState);
      })
      .catch(err => {
        this.global.commonFuncs.openPortal('错误', '小组列表加载失败', 'red');
        console.log(err);
      })
  }

  componentDidMount() {
    setTitle('小组');
    if (!this.props.match.params.page) this.fetchGroupList(true, 1);
    else this.fetchGroupList(true, this.props.match.params.page);
  }

  gotoDetails(index: number) {
    if (this.disableNavi) {
      this.disableNavi = false;
      return;
    }
    this.props.history.push(`/details/group/${index}`);
  }

  editGroup(id: number) {
    this.disableNavi = true;
    this.props.history.push(`/edit/group/${id}`);
  }

  renderGroupList() {
    return <>
      <Table color='black' selectable>
        <Table.Header>
          <Table.Row>
            <Table.HeaderCell>编号</Table.HeaderCell>
            <Table.HeaderCell>名称</Table.HeaderCell>
            <Table.HeaderCell>创建者</Table.HeaderCell>
            <Table.HeaderCell>创建时间</Table.HeaderCell>
            <Table.HeaderCell>公开性</Table.HeaderCell>
            {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.HeaderCell textAlign='center'>操作</Table.HeaderCell> : null}
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {
            this.state.groupList.groups.map((v, i) =>
              <Table.Row key={i} onClick={() => this.gotoDetails(v.id)} style={{ cursor: 'pointer' }}>
                <Table.Cell>{v.id}</Table.Cell>
                <Table.Cell>{v.name}</Table.Cell>
                <Table.Cell>{v.userName}</Table.Cell>
                <Table.Cell>{v.creationTime.toLocaleString(undefined, { hour12: false })}</Table.Cell>
                <Table.Cell>{v.isPrivate ? '公开' : '私有'}</Table.Cell>
                {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Table.Cell textAlign='center'><Button.Group><Button onClick={() => this.editGroup(v.id)} color='grey'>编辑</Button><Button color='red'>删除</Button></Button.Group></Table.Cell> : null}
              </Table.Row>)
          }
        </Table.Body>
      </Table>
    </>;
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

    return <>
      <Form id='filterForm'>
        <Form.Group widths={'equal'}>
          <Form.Field width={4}>
            <Label>小组编号</Label>
            <Input fluid name='id' type='number' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <Label>小组名称</Label>
            <Input fluid name='name' onChange={() => this.idRecord.clear()}></Input>
          </Form.Field>
          <Form.Field width={8}>
            <Label>小组状态</Label>
            <Select onChange={(_event, data) => { this.setState({ statusFilter: data.value as number[] } as GroupState); this.idRecord.clear(); }} fluid name='status' multiple defaultValue={[0, 1]} options={[{ text: '已加入', value: 0 }, { text: '未加入', value: 1 }]}></Select>
          </Form.Field>
          <Form.Field width={4}>
            <Label>小组操作</Label>
            <Button.Group fluid>
              <Button type='button' primary onClick={() => this.fetchGroupList(true, 1)}>筛选</Button>
              {this.global.userInfo.userId && isTeacher(this.global.userInfo.privilege) ? <Button type='button' secondary onClick={() => this.editGroup(0)}>添加</Button> : null}
            </Button.Group>
          </Form.Field>
        </Form.Group>
      </Form>
      {this.renderGroupList()}
      {this.state.loaded ? (this.state.groupList.groups.length === 0 ? <p>没有数据</p> : null) : placeHolder}
      <div style={{ textAlign: 'center' }}>
        <Pagination
          activePage={this.state.page}
          onPageChange={(_event, data) => this.fetchGroupList(false, data.activePage as number)}
          size='small'
          siblingRange={3}
          boundaryRange={1}
          totalPages={this.state.groupList.totalCount === 0 ? 0 : Math.floor(this.state.groupList.totalCount / 10) + (this.state.groupList.totalCount % 10 === 0 ? 0 : 1)}
          firstItem={null}
          lastItem={null}
        />
      </div>
    </>;
  }
}