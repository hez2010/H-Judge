<template>
    <v-card>
        <v-card-title primary-title>
            <h2>重置密码</h2>
        </v-card-title>
        <v-card-text>
            <v-form ref="form" v-model="valid" lazy-validation>
                <v-text-field v-model="username"
                              :rules="usernameRules"
                              label="用户名"
                              :counter="30"
                              v-if="!sent"
                              required>
                </v-text-field>
                <v-text-field v-model="email"
                              :rules="emailRules"
                              label="邮箱地址"
                              v-if="!sent"
                              required>
                </v-text-field>
                <h3 v-if="sent"><v-icon color="success">done</v-icon>验证码已发送至邮箱</h3>
                <v-text-field v-model="token"
                              :rules="tokenRules"
                              label="验证码"
                              v-if="sent"
                              required>
                </v-text-field>
                <v-text-field :rules="passwordRules"
                              v-model="password"
                              label="密码"
                              type="password"
                              v-if="sent"
                              required>
                </v-text-field>
                <v-text-field :rules="confirmPasswordRules"
                              v-model="confirmPassword"
                              label="确认密码"
                              type="password"
                              v-if="sent"
                              required>
                </v-text-field>
                <v-btn :disabled="!valid"
                       @click="sendEmail"
                       color="primary"
                       v-if="!sent">
                    确定
                </v-btn>
                <v-btn :disabled="!valid || submitting"
                       @click="reset"
                       color="primary"
                       v-else>
                    确定
                </v-btn>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script type="text/javascript" src="./password.js"></script>