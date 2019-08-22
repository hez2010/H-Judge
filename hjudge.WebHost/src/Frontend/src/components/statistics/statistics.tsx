import * as React from 'reactn';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { GlobalState } from '../../interfaces/globalState';

interface StatisticsProps {
  problemId?: number, // unused
  contestId?: number, // unused
  groupId?: number // unused
}

interface StatisticsItemModel {
  problemId: number,
  resultId: number,
  problemName: number,
  userId: number,
  userName: number,
  resultType: string,
  time: Date
}

interface StatisticsListModel {
  statistics: StatisticsItemModel[],
  totalCount: number
}

interface StatisticsState {

}

export default class Statistics extends React.Component<StatisticsProps, StatisticsState, GlobalState> {
  constructor() {
    super();
    

  }

  componentDidMount() {

  }

  render() {
    return <></>;
  }
}