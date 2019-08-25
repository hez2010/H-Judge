import * as React from 'react';
import { Modal, Input, Button, Form } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Put } from '../../utils/requestHelper';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { getTargetState } from '../../utils/reactnHelper';
import { GlobalState } from '../../interfaces/globalState';
import { useGlobal } from 'reactn';
import { CommonProps } from '../../interfaces/commonProps';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';

interface RegisterProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

const Register = (props: CommonProps & RegisterProps) => {
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));

  const register = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#registerForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Put('/user/register', SerializeForm(form))
        .then(res => tryJson(res))
        .then(data => {
          element.disabled = false;
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          props.closeModal();
          commonFuncs.refreshUserInfo();
          commonFuncs.openPortal('提示', '注册并登录成功', 'green');
        })
        .catch(err => {
          commonFuncs.openPortal('错误', '注册失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  }

  return (
    <>
      <Modal size='tiny' open={props.modalOpen} closeIcon onClose={props.closeModal}>
        <Modal.Header>注册</Modal.Header>
        <Modal.Content>
          <Form id='registerForm'>
            <Form.Field required>
              <label>用户名</label>
              <Input name='userName' required></Input>
            </Form.Field>
            <Form.Field required>
              <label>邮箱</label>
              <Input name='email' required type='email'></Input>
            </Form.Field>
            <Form.Field required>
              <label>姓名</label>
              <Input name='name' required></Input>
            </Form.Field>
            <Form.Field required>
              <label>密码</label>
              <Input id='password' name='password' required type='password' placeholder='至少 6 位，包含大小写字母及数字'></Input>
            </Form.Field>
            <Form.Field required>
              <label>再次输入密码</label>
              <Input id='confirmPassword' name='confirmPassword' error required type='password'></Input>
            </Form.Field>

            <Button onClick={register} primary>注册</Button>
          </Form>
        </Modal.Content>
      </Modal>
    </>
  );
}

export default Register;