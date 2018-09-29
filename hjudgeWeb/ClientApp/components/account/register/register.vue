<template>
    <v-card>
        <v-card-title primary-title>
            <h2>注册 H::Judge</h2>
        </v-card-title>
        <v-card-text>
            <v-form ref="form" v-model="valid" lazy-validation>
                <v-text-field v-model="username"
                              :rules="usernameRules"
                              :counter="15"
                              label="用户名"
                              required>
                </v-text-field>
                <v-text-field v-model="email"
                              :rules="emailRules"
                              label="邮箱"
                              required>
                </v-text-field>
                <v-text-field :rules="passwordRules"
                              v-model="password"
                              label="密码"
                              type="password"
                              @change="passwordChanged"
                              required>
                </v-text-field>
                <v-text-field :rules="confirmPasswordRules"
                              v-model="confirmPassword"
                              label="确认密码"
                              type="password"
                              required>
                </v-text-field>
                <v-btn :disabled="!valid || submitting"
                       @click="register"
                       color="primary">
                    注册
                </v-btn>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script src="./register.js"></script>