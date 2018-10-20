<template>
    <v-card>
        <v-card-title primary-title>
            <h2>验证邮箱地址</h2>
        </v-card-title>
        <v-card-text>
            <v-form ref="form" v-model="valid" lazy-validation>
                <h3><v-icon color="success">done</v-icon>验证码已发送至邮箱</h3>
                <v-text-field v-model="token"
                              :rules="tokenRules"
                              label="验证码"
                              required>
                </v-text-field>
                <v-btn :disabled="!valid || submitting"
                       @click="confirmEmail"
                       color="primary">
                    确定
                </v-btn>
            </v-form>
        </v-card-text>
    </v-card>
</template>

<script type="text/javascript" src="./emailConfirm.js"></script>