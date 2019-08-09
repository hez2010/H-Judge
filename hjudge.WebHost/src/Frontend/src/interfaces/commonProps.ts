import { SemanticCOLORS } from 'semantic-ui-react';
import { History, Location } from 'history';
import { UserInfo } from './userInfo';
import { match } from 'react-router';

export interface CommonProps {
  openPortal: ((header: string, message: string, color: SemanticCOLORS) => void),
  refreshUserInfo: (() => void),
  userInfo: UserInfo,
  match: match<any>,
  history: History<any>,
  location: Location<any>
}