import * as React from 'react';
import { CommonProps } from '../../interfaces/commonProps';

interface StatisticsProps extends CommonProps {
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

export default class Statistics extends React.Component<StatisticsProps, StatisticsState> {
  constructor(props: StatisticsProps) {
    super(props);
    

  }

  componentDidMount() {

  }
}