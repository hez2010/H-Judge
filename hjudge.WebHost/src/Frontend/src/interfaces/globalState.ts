import { UserInfo } from './userInfo';
import { CommonFuncs } from './commonFuncs';

export interface GlobalState {
  userInfo?: UserInfo,
  setUserInfo?: (u: UserInfo) => void,
  commonFuncs?: CommonFuncs
}