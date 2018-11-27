<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>消息详情</h2>
            </v-card-title>
            <v-card-text>
                <v-container v-if="loading">加载中...</v-container>
                <v-container v-else>
                    <h3>
                        {{message.title}}
                    </h3>
                    <br />
                    <div id="details_content" class="cke_contents" v-highlight v-html="message.content"></div>
                </v-container>
            </v-card-text>
            <v-card-actions>
                <v-container>
                    <v-layout v-if="!loading">
                        <div>
                            <v-icon v-if="message.type != 1">account_circle</v-icon>
                            <router-link v-if="message.type != 1" :to="{ path: '/Account/' + message.userId }">{{message.userName}}</router-link>
                            <v-icon>access_time</v-icon>
                            <span>{{message.sendTime}}</span>
                        </div>
                        <v-spacer></v-spacer>
                        <v-tooltip bottom v-if="message.type != 1 && message.direction == 2">
                            <v-btn icon slot="activator" :to="{ path: '/NewMessage/' + message.id }">
                                <v-icon color="primary">send</v-icon>
                            </v-btn>
                            <span>回复</span>
                        </v-tooltip>
                    </v-layout>
                </v-container>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./messageDetails.js"></script>