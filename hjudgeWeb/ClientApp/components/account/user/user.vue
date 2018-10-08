<template>
    <v-container grid-list-md text-xs-center>
        <v-layout row wrap>
            <v-flex xs12>
                <v-card>
                    <v-card-title primary-title>
                        <h2>用户信息</h2>
                    </v-card-title>
                    <v-card-text>
                        <v-container v-if="!loading">
                            <v-layout wrap v-bind="isColumn">
                                <v-flex xs3>
                                    <v-avatar color="grey lighten-4" size="80">
                                        <img :src="avatar" />
                                    </v-avatar>
                                    <div>
                                        <v-chip>
                                            <span v-if="user && user.name && user.privilege >= 1 && user.privilege <= 3">
                                                <span>{{userInfo.userName}} ({{userInfo.name}}) ({{privilege}})</span>
                                            </span>
                                            <span v-else>{{userInfo.userName}} ({{privilege}})</span>
                                        </v-chip>
                                    </div>
                                    <br />
                                    <div>
                                        <span>金币</span>
                                        <v-chip>
                                            <span>{{userInfo.coins}}</span>
                                        </v-chip>
                                        <span>经验</span>
                                        <v-chip>
                                            <span>{{userInfo.experience}}</span>
                                        </v-chip>
                                    </div>
                                </v-flex>
                                <v-flex xs9>
                                    <v-card flat>
                                        <v-card-title primary-title>
                                            <h3>资料</h3>
                                        </v-card-title>
                                        <v-card-text>
                                            <v-container>
                                                <template v-for="item in userInfo.otherInfo">
                                                    <v-layout wrap>
                                                        <v-flex xs4>
                                                            <strong>{{item.name}}</strong>
                                                        </v-flex>
                                                        <v-flex>
                                                            <p>{{item.value}}</p>
                                                        </v-flex>
                                                    </v-layout>
                                                </template>
                                            </v-container>
                                        </v-card-text>
                                    </v-card>
                                </v-flex>
                            </v-layout>
                        </v-container>
                        <v-container v-else>
                            <p>加载中...</p>
                        </v-container>
                    </v-card-text>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script type="text/javascript" src="./user.js"></script>