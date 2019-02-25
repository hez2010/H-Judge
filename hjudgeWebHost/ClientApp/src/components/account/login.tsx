import * as React from 'react';
import { Modal, Input, Button, Form, Label, Checkbox } from 'semantic-ui-react';
import { SerializeForm } from '../../utils/formHelper';
import { Post } from '../../utils/requestHelper';

interface LoginProps {
  modalOpen: boolean,
  closeModal: (() => void)
}

export default class Login extends React.Component<LoginProps> {
  constructor(props: any) {
    super(props);
    this.login = this.login.bind(this);
  }

  login() {
    let form = document.getElementById('loginForm') as HTMLFormElement;
    if (form.reportValidity()) {
      Post('/Account/Login', SerializeForm(form))
        .then(res => res.json())
        .then(data => console.log(data));
    }
  }

  render() {
    return (
      <>
        <Modal size='tiny' open={this.props.modalOpen} closeIcon onClose={this.props.closeModal}>
          <Modal.Header>登录</Modal.Header>
          <Modal.Content>
            <Form id='loginForm'>
              <Form.Field>
                <Label>用户名</Label>
                <Input name='username' required></Input>
              </Form.Field>
              <Form.Field>
                <Label>密码</Label>
                <Input name='password' required type='password'></Input>
              </Form.Field>
              <Form.Field>
                <Checkbox name='rememberMe' label='记住登录状态'></Checkbox>
              </Form.Field>
              <Button onClick={this.login}>登录</Button>
            </Form>
          </Modal.Content>
        </Modal>
      </>
    );
  }
}