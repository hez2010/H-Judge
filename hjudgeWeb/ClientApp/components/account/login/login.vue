<template>
    <v-card>
        <v-card-title primary-title>
            <h2>登录 H::Judge</h2>
        </v-card-title>
        <v-card-text>
            <v-form ref="form" v-model="valid" lazy-validation>
                <v-text-field v-model="username"
                              :rules="usernameRules"
                              :counter="10"
                              label="用户名"
                              required>
                </v-text-field>
                <v-text-field :rules="passwordRules"
                              v-model="password"
                              label="密码"
                              type="password"
                              required>
                </v-text-field>
                <v-checkbox v-model="rememberMe" label="自动登录">
                </v-checkbox>
                <v-btn :disabled="!valid || submitting"
                       @click="login"
                       color="primary">
                    登录
                </v-btn>
                <a>忘记密码</a>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script type="text/javascript" src="./login.js"></script>