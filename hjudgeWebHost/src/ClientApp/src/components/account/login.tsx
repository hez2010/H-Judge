import * as React from 'react';
import { Modal, Input, Button, Form, Label, Checkbox, SemanticCOLORS } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Post } from '../../utils/requestHelper';
import { ResultModel } from '../../interfaces/resultModel';
import { CommonProps } from '../../interfaces/commonProps';

interface LoginProps extends CommonProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

export default class Login extends React.Component<LoginProps> {
  constructor(props: any) {
    super(props);
    this.login = this.login.bind(this);
  }

  login(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    let element = event.target as HTMLButtonElement;
    let form = document.querySelector('#loginForm') as HTMLFormElement;
    if (form.reportValidity()) {
      element.disabled = true;
      Post('/Account/Login', SerializeForm(form))
        .then(res => res.json())
        .then(data => {
          let result = data as ResultModel;
          if (result.succeeded) {
            this.props.refreshUserInfo();
            this.props.closeModal();
            this.props.openPortal('提示', '登录成功', 'green');
          }
          else {
            this.props.openPortal('错误', `${result.errorMessage} (${result.errorCode})`, 'red');
          }
          element.disabled = false;
        })
        .catch(err => {
          this.props.openPortal('错误', '登录失败', 'red');
          element.disabled = false;
          console.log(err);
        })
    }
    
  }

  render() {
    return (
      <>
        <Modal size='tiny' open={this.props.modalOpen} closeIcon onClose={this.props.closeModal}>
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
              <Button onClick={this.login} primary>登录</Button>
            </Form>
          </Modal.Content>
        </Modal>
      </>
    );
  }
}