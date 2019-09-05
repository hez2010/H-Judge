import * as React from 'react';
import { Modal, Input, Button, Form, Checkbox } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Post } from '../../utils/requestHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { useGlobal } from 'reactn';
import { getTargetState } from '../../utils/reactnHelper';
import { GlobalState } from '../../interfaces/globalState';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';

interface LoginProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

const Login = (props: LoginProps) => {
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  const login = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#loginForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Post('/user/login', SerializeForm(form))
        .then(tryJson)
        .then(data => {
          element.disabled = false;
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          commonFuncs.refreshUserInfo();
          props.closeModal();
          commonFuncs.openPortal('提示', '登录成功', 'green');
        })
        .catch(err => {
          commonFuncs.openPortal('错误', '登录失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  }

  const forgotPassword = () => {
    commonFuncs.openPortal('提示', '此功能正在开发中，敬请期待', 'blue');
  }

  return (
    <>
      <Modal size='tiny' open={props.modalOpen} closeIcon onClose={props.closeModal}>
        <Modal.Header>登录</Modal.Header>
        <Modal.Content>
          <Form id='loginForm'>
            <Form.Field required>
              <label>用户名</label>
              <Input name='userName' required></Input>
            </Form.Field>
            <Form.Field required>
              <label>密码</label>
              <Input name='password' required type='password'></Input>
            </Form.Field>
            <Form.Field>
              <Checkbox name='rememberMe' label='记住登录状态'></Checkbox>
            </Form.Field>
            <Button onClick={login} primary>登录</Button>
            <a href="#" onClick={forgotPassword} style={{ float: 'right' }}>忘记密码</a>
          </Form>
        </Modal.Content>
      </Modal>
    </>
  );
};

export default Login;