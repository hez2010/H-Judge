<template>
    <v-container>
        <v-card>
            <v-tabs v-model="active" right>
                <v-card-title primary-title>
                    <h2>题目详情</h2>
                </v-card-title>
                <v-spacer></v-spacer>
                <v-tab :key="1">
                    信息
                </v-tab>
                <v-tab :key="2">
                    描述
                </v-tab>
                <v-tab :key="3">
                    提交
                </v-tab>
                <v-tab :key="4">
                    状态
                </v-tab>
                <v-tab-item :key="1">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-layout wrap>
                                <v-flex xs4>
                                    <strong>题目编号</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.id}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>题目名称</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.name}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>创建时间</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.creationTime}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>题目难度</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.level}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>题目类型</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.type}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>出题用户</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p><router-link :to="{ path: '/Account/' + problem.userId }">{{problem.userName}}</router-link></p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>当前状态</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.status}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>通过数量</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.acceptCount}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>提交数量</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.submissionCount}}</p>
                                </v-flex>
                                <v-flex xs4>
                                    <strong>通过比率</strong>
                                </v-flex>
                                <v-flex xs8>
                                    <p>{{problem.submissionCount === 0 ? 0 : Math.round(problem.acceptCount * 10000 / problem.submissionCount) / 100}} %</p>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="2">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container class="cke_contents" v-else v-highlight v-html="problem.description"></v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="3">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-form v-if="user.isSignedIn" ref="form" v-model="valid" lazy-validation>
                                <v-select :items="problem.languages"
                                          label="语言"
                                          v-if="problem.rawType === 1"
                                          v-model="language"
                                          item-text="name"
                                          item-value="name"
                                          :hint="language.information"
                                          persistent-hint
                                          return-object
                                          single-line
                                          required
                                          :rules="languageRules">
                                </v-select>
                                <v-textarea outline
                                            v-model="content"
                                            :rules="contentRules"
                                            label="提交内容"
                                            required>
                                </v-textarea>
                                <v-btn color="primary" :disabled="!valid || submitting" @click="submit">提交</v-btn>
                            </v-form>
                            <p v-else>请登录后再操作</p>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
                <v-tab-item :key="4">
                    <v-card-text>
                        <p>正在跳转...</p>
                    </v-card-text>
                </v-tab-item>
            </v-tabs>
        </v-card>
        <remote-script src="https://cdn.hjudge.com/hjudge/lib/MathJax-2.7.5/MathJax.js?config=TeX-MML-AM_CHTML"></remote-script>
    </v-container>
</template>

<script type="text/javascript" src="./problemDetails.js"></script>