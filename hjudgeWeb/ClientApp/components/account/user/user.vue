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
                                        <img :src="`${'/Account/GetUserAvatar?userId=' + userInfo.id}`" />
                                    </v-avatar>
                                    <div>
                                        <v-chip>
                                            <span v-if="user && userInfo.name && user.privilege >= 1 && user.privilege <= 3">
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
                                        <div v-if="bottomNav === '1'">
                                            <h3>用户资料</h3>
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
                                        </div>
                                        <div v-else-if="bottomNav === '2'">
                                            <h3>已解决的题目</h3>
                                            <v-container>
                                                <p v-if="loadingProblems">加载中...</p>
                                                <div v-else>
                                                    <p v-if="problemSet.length === 0">无</p>
                                                    <v-layout v-if="problemSet.length >= 6" v-for="i in parseInt(problemSet.length / 6)">
                                                        <v-flex xs2 v-for="j in 6">
                                                            <router-link :to="{ path: '/ProblemDetails/' + problemSet[(i - 1) * 6 + j - 1] }">
                                                                #{{problemSet[(i - 1) * 6 + j - 1]}}
                                                            </router-link>
                                                        </v-flex>
                                                    </v-layout>
                                                    <v-layout v-if="problemSet.length % 6 !== 0">
                                                        <v-flex xs2 v-for="j in problemSet.length % 6">
                                                            <router-link :to="{ path: '/ProblemDetails/' + problemSet[parseInt(problemSet.length / 6) * 6 + j - 1] }">
                                                                #{{problemSet[parseInt(problemSet.length / 6) * 6 + j - 1]}}
                                                            </router-link>
                                                        </v-flex>
                                                    </v-layout>
                                                </div>
                                            </v-container>
                                        </div>
                                        <br />
                                        <br />
                                        <br />
                                        <v-bottom-nav :active.sync="bottomNav"
                                                      :value="true"
                                                      absolute
                                                      color="transparent">
                                            <v-btn color="teal"
                                                   flat
                                                   value="1">
                                                <span>用户信息</span>
                                                <v-icon>info</v-icon>
                                            </v-btn>
                                            <v-btn color="teal"
                                                   flat
                                                   value="2">
                                                <span>解决题目</span>
                                                <v-icon>code</v-icon>
                                            </v-btn>
                                        </v-bottom-nav>
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