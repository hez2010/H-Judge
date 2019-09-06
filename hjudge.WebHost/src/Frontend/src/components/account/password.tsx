import * as React from 'react';
import { useGlobal } from 'reactn';
import { GlobalState } from '../../interfaces/globalState';
import { CommonFuncs } from '../../interfaces/commonFuncs';
import { SerializeForm } from '../../utils/formHelper';
import { ErrorModel } from '../../interfaces/errorModel';
import { tryJson } from '../../utils/responseHelper';
import { Post } from '../../utils/requestHelper';
import { getTargetState } from '../../utils/reactnHelper';
import { Modal, Form, Input, Checkbox, Button } from 'semantic-ui-react';

interface PasswordProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

const Password = (props: PasswordProps) => {
  const [commonFuncs] = getTargetState<CommonFuncs>(useGlobal<GlobalState>('commonFuncs'));
  const [sent, setSent] = React.useState(false);

  const sendResetEmail = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#emailForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Post('/user/reset-email', SerializeForm(form))
        .then(tryJson)
        .then(data => {
          element.disabled = false;
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          setSent(true);
          commonFuncs.openPortal('提示', '重置密码邮件已发送，请根据邮件中的提示重置密码', 'green');
        })
        .catch(err => {
          commonFuncs.openPortal('错误', '重置密码邮件发送失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  };

  const resetPassword = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#resetForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Post('/user/reset-password', SerializeForm(form))
        .then(tryJson)
        .then(data => {
          element.disabled = false;
          let error = data as ErrorModel;
          if (error.errorCode) {
            commonFuncs.openPortal(`错误 (${error.errorCode})`, `${error.errorMessage}`, 'red');
            return;
          }
          setSent(true);
          close();
          commonFuncs.openPortal('提示', '密码重置成功', 'green');
        })
        .catch(err => {
          commonFuncs.openPortal('错误', '密码重置失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
  };

  const inputModal = <Form id='emailForm'>
    <Form.Field required>
      <label>邮箱</label>
      <Input name='email' type="email" required></Input>
    </Form.Field>
    <Button onClick={sendResetEmail} primary>重置密码</Button>
  </Form>;

  const close = () => {
    setSent(false);
    props.closeModal();
  }

  const resetModal = <>
    <Form id='resetForm'>
      <Form.Field required>
        <label>邮箱</label>
        <Input name='email' type="email" required></Input>
      </Form.Field>
      <Form.Field required>
        <label>密码</label>
        <Input id='password' name='password' required type='password' placeholder='至少 6 位，包含大小写字母及数字'></Input>
      </Form.Field>
      <Form.Field required>
        <label>再次输入密码</label>
        <Input id='confirmPassword' name='confirmPassword' error required type='password'></Input>
      </Form.Field>
      <Form.Field required>
        <label>验证码</label>
        <Input id='token' name='token' error required></Input>
      </Form.Field>
      <Button onClick={resetPassword} primary>重置密码</Button>
    </Form>
  </>;

  return <Modal size='tiny' open={props.modalOpen} closeIcon onClose={close}>
    <Modal.Header>重置密码</Modal.Header>
    <Modal.Content>
      {
        sent ? resetModal : inputModal
      }
    </Modal.Content>
  </Modal>;
}

export default Password;