import * as React from 'react';
import { CommonProps } from '../../interfaces/commonProps';
import { ResultModel } from '../../interfaces/resultModel';

interface ContestDetailsProps extends CommonProps { }

enum ContestType {
  generic, lastSubmit, penalty
}

enum ResultDisplayMode {
  intime, afterContest, never
}

enum ResultDisplayType {
  detailed, summary
}

interface ContestConfig {
  type: ContestType,
  submissionLimit: number,
  resultMode: ResultDisplayMode,
  resultType: ResultDisplayType,
  showRank: boolean,
  autoStopRank: boolean,
  languages: string,
  canMakeResultPublic: boolean,
  canDiscussion: boolean
}

interface ContestModel extends ResultModel {
  id: number,
  name: string,
  startTime: Date,
  endTime: Date,
  password: string,
  userId: string,
  userName: string,
  description: string,
  config: ContestConfig,
  hidden: boolean,
  upvote: number,
  downvote: number,
  currentTime: Date
}

interface ContestDetailsState {
  contest: ContestModel
}

export default class ContestDetails extends React.Component<ContestDetailsProps, ContestDetailsState> {
  constructor(props: ContestDetailsProps) {
    super(props);
    this.state = {
      contest: {
        id: 0,
        startTime: new Date(),
        endTime: new Date(),
        currentTime: new Date(Date.now()),
        config: {
          type: ContestType.generic,
          autoStopRank: false,
          canDiscussion: false,
          canMakeResultPublic: false,
          languages: '',
          resultMode: ResultDisplayMode.intime,
          resultType: ResultDisplayType.detailed,
          showRank: true,
          submissionLimit: 0
        },
        description: '',
        downvote: 0,
        upvote: 0,
        hidden: false,
        name: '',
        password: '',
        userId: '',
        userName: ''
      }
    }
  }

  contestContent() {

  }

  render() {
    return <></>;
  }
}