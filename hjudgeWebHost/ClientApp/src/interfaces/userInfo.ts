import { ResultModel } from "./resultModel";

export interface UserInfo extends ResultModel {
  userId: string,
  userName: string,
  privilege: number,
  name: string,
  signedIn: boolean,
  email: string,
  phoneNumber: string,
  emailConfirmed: boolean,
  phoneNumberConfirmed: boolean,
  otherInfo?: OtherInfo[]
}

export interface OtherInfo {
  key: string,
  name: string,
  value: string
}