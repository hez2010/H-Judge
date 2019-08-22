import * as React from 'react';
import { Modal, Input, Button, Form, Label, Checkbox } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Post } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { useGlobal } from 'reactn';
import { getTargetState } from '../../utils/reactnHelper';
import { GlobalState } from '../../interfaces/globalState';
import { CommonProps } from '../../interfaces/commonProps';

interface LoginProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

const Login = (props: CommonProps & LoginProps) => {
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  const login = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#loginForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Post('/user/login', SerializeForm(form))
        .then(res => res.json())
        .then(data => {
          let result = data as ResultModel;
          if (result.succeeded) {
            commonFuncs.refreshUserInfo();
            props.closeModal();
            commonFuncs.openPortal('提示', '登录成功', 'green');
          }
          else {
            commonFuncs.openPortal(`错误 (${result.errorCode})`, `${result.errorMessage}`, 'red');
          }
          element.disabled = false;
        })
        .catch(err => {
          commonFuncs.openPortal('错误', '登录失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  }
  return (
    <>
      <Modal size='tiny' open={props.modalOpen} closeIcon onClose={props.closeModal}>
        <Modal.Header>登录</Modal.Header>
        <Modal.Content>
          <Form id='loginForm'>
            <Form.Field required>
              <Label>用户名</Label>
              <Input name='userName' required></Input>
            </Form.Field>
            <Form.Field required>
              <Label>密码</Label>
              <Input name='password' required type='password'></Input>
            </Form.Field>
            <Form.Field>
              <Checkbox name='rememberMe' label='记住登录状态'></Checkbox>
            </Form.Field>
            <Button onClick={login} primary>登录</Button>
          </Form>
        </Modal.Content>
      </Modal>
    </>
  );
};

export default Login;