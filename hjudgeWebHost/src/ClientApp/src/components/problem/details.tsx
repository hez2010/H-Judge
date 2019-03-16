import * as React from 'react';
import { match } from 'react-router';
import { History, Location } from 'history';
import { SemanticCOLORS } from 'semantic-ui-react';

interface ProblemDetailsProps {
  match: match<any>,
  history: History<any>,
  location: Location<any>,
  openPortal: ((header: string, message: string, color: SemanticCOLORS) => void),
  contestId?: number,
  groupId?: number
}

export default class ProblemDetails extends React.Component<ProblemDetailsProps> {
  constructor(props: ProblemDetailsProps) {
    super(props);
  }

  fetchDetail() {

  }

  componentDidMount() {

  }

  render() {
    return <></>;
  }
}