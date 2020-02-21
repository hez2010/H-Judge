import * as React from 'react';
import { useGlobal } from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { getTargetState } from '../../utils/reactnHelper';

export interface UserQueryResultModel {
  userId: string,
  userName: string,
  email: string,
  name: string
}

interface UserFinderProps {
  onSelect: ((userId: UserQueryResultModel) => void)
}

const UserFinder = (props: UserFinderProps) => {
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));
  // TODO: a user finder. Find a user by userName, name, email, phone and etc.
};