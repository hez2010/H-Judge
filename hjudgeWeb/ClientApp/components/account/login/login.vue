<template>
    <v-card>
        <v-card-title class="headline grey lighten-2" primary-title>
            <h3>登录 H::Judge</h3>
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
                <v-btn :disabled="!valid || submitting"
                       @click="login"
                       color="primary">
                    登录
                </v-btn>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script src="./login.js"></script>