<template>
    <v-container>
        <v-card>
            <v-tabs v-model="active" right>
                <v-card-title primary-title>
                    <h2>比赛详情</h2>
                </v-card-title>
                <v-spacer></v-spacer>
                <v-tab :key="1">
                    信息
                </v-tab>
                <v-tab :key="2">
                    描述
                </v-tab>
                <v-tab :key="3">
                    题目
                </v-tab>
                <v-tab :key="4">
                    状态
                </v-tab>
                <v-tab :key="5">
                    排名
                </v-tab>
                <v-tab-item :key="1">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-layout wrap>
                                <v-flex xs4>
                                    <strong>比赛编号</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.id}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>比赛名称</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.name}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>开始时间</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.startTime}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>结束时间</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.endTime}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>题目数量</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.problemCount}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>创建用户</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.userName}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>当前状态</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{contest.status}}</p>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="2">
                    <v-card-text>
                        <v-container v-if="loading"><p>加载中...</p></v-container>
                        <v-container v-else v-html="contest.description"></v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="3">
                    <v-card-text>
                        <v-container v-if="loading"><p>加载中...</p></v-container>
                        <v-container v-else-if="verified || (!contest.password || contest.password === '')">
                            <h3>题目列表</h3>
                            <v-data-table :headers="headers"
                                          :items="problems"
                                          hide-actions>
                                <template slot="items" slot-scope="props">
                                    <td>{{ props.item.id }}</td>
                                    <td><a @click="toDetails(props.item.id)">{{ props.item.name }}</a></td>
                                    <td>{{ props.item.creationTime }}</td>
                                    <td>{{ props.item.type }}</td>
                                    <td>{{ props.item.level }}</td>
                                    <td>{{ props.item.status }}</td>
                                    <td>{{ props.item.acceptCount }}</td>
                                    <td>{{ props.item.submissionCount }}</td>
                                </template>
                                <template slot="no-data">
                                    <p v-if="loadingProblem">正在加载...</p>
                                    <p v-else>没有数据 :(</p>
                                </template>
                            </v-data-table>
                            <br />
                            <div class="text-xs-center">
                                <v-pagination circle v-model="page" :length="pageCount" :total-visible="7"></v-pagination>
                            </div>
                        </v-container>
                        <v-container v-else>
                            <h4>请输入密码</h4>
                            <v-form ref="form" v-model="valid" lazy-validation>
                                <v-text-field v-model="password"
                                              :rules="passwordRules"
                                              label="密码"
                                              typeof="password"
                                              required>
                                </v-text-field>
                                <v-btn :disabled="!valid"
                                       @click="verify"
                                       color="primary">
                                    确定
                                </v-btn>
                            </v-form>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="4">
                    <v-card-text>
                        <v-container>
                            <p>正在跳转...</p>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="5">
                    <v-card-text>
                        <v-container>
                            <p>正在跳转...</p>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
            </v-tabs>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./contestDetails.js"></script>