<template>
    <v-container>
        <v-card>
            <v-card-title>
                <h2>系统设置</h2>
            </v-card-title>
            <v-card-text>
                <p v-if="loading">加载中...</p>
                <v-container v-else>
                    <v-form v-model="valid" ref="form" lazy-validation>
                        <p><strong>目标系统：</strong>{{config.system}}</p>
                        <v-text-field outline
                                      v-model="config.environments"
                                      label="PATH"
                                      placeholder="多个路径请以 ; 分隔">
                        </v-text-field>
                        <v-layout>
                            <v-checkbox label="开启讨论功能"
                                        v-model="config.canDiscussion">
                            </v-checkbox>
                        </v-layout>
                        <v-layout row>
                            <h3>语言配置</h3>
                            <v-spacer></v-spacer>
                            <v-tooltip bottom>
                                <v-btn icon @click="add" slot="activator">
                                    <v-icon color="primary">add</v-icon>
                                </v-btn>
                                <span>添加</span>
                            </v-tooltip>
                        </v-layout>
                        <p v-if="config.languages.length === 0">没有配置</p>
                        <template v-for="(item, index) in config.languages">
                            <v-tabs right :key="index">
                                <v-spacer></v-spacer>
                                <v-tab :key="1">
                                    语言选项
                                </v-tab>
                                <v-tab :key="2">
                                    编译器
                                </v-tab>
                                <v-tab :key="3">
                                    静态检查
                                </v-tab>
                                <v-tab :key="4">
                                    运行选项
                                </v-tab>
                                <v-tooltip bottom>
                                    <v-btn icon @click="remove(index)" slot="activator">
                                        <v-icon color="red">delete</v-icon>
                                    </v-btn>
                                    <span>删除</span>
                                </v-tooltip>
                                <v-tab-item :key="1">
                                    <v-text-field v-model="item.name"
                                                  label="语言名称"
                                                  required
                                                  :rules="requireRules">
                                    </v-text-field>
                                    <v-text-field v-model="item.extensions"
                                                  label="扩展名"
                                                  required
                                                  :rules="requireRules">
                                    </v-text-field>
                                    <v-text-field v-model="item.information"
                                                  label="详情信息">
                                    </v-text-field>
                                    <v-select v-model="item.syntaxHighlight"
                                              :items="supportLanguages"
                                              single-line
                                              label="语法高亮模板">

                                    </v-select>
                                </v-tab-item>
                                <v-tab-item :key="2">
                                    <v-text-field v-model="item.compilerExec"
                                                  label="编译器">
                                    </v-text-field>
                                    <v-text-field v-model="item.compilerArgs"
                                                  label="编译参数">
                                    </v-text-field>
                                    <v-text-field v-model="item.compilerProblemMatcher"
                                                  label="编译问题匹配">
                                    </v-text-field>
                                    <v-text-field v-model="item.compilerDisplayFormat"
                                                  label="编译问题显示格式">
                                    </v-text-field>
                                    <v-layout>
                                        <v-flex xs6>
                                            <v-checkbox label="编译时读取标准输出"
                                                        v-model="item.compilerReadStdOutput">
                                            </v-checkbox>
                                        </v-flex>
                                        <v-flex xs6>
                                            <v-checkbox label="编译时读取标准错误"
                                                        v-model="item.compilerReadStdError">
                                            </v-checkbox>
                                        </v-flex>
                                    </v-layout>
                                </v-tab-item>
                                <v-tab-item :key="3">
                                    <v-text-field v-model="item.staticCheckExec"
                                                  label="静态检查器">
                                    </v-text-field>
                                    <v-text-field v-model="item.staticCheckArgs"
                                                  label="检查参数">
                                    </v-text-field>
                                    <v-text-field v-model="item.staticCheckProblemMatcher"
                                                  label="检查问题匹配">
                                    </v-text-field>
                                    <v-text-field v-model="item.staticCheckDisplayFormat"
                                                  label="检查问题显示格式">
                                    </v-text-field>
                                    <v-layout>
                                        <v-flex xs6>
                                            <v-checkbox label="检查时读取标准输出"
                                                        v-model="item.staticCheckReadStdOutput">
                                            </v-checkbox>
                                        </v-flex>
                                        <v-flex xs6>
                                            <v-checkbox label="检查时读取标准错误"
                                                        v-model="item.staticCheckReadStdError">
                                            </v-checkbox>
                                        </v-flex>
                                    </v-layout>
                                </v-tab-item>
                                <v-tab-item :key="4">
                                    <v-text-field v-model="item.runExec"
                                                  label="执行文件"
                                                  required
                                                  :rules="requireRules">
                                    </v-text-field>
                                    <v-text-field v-model="item.runArgs"
                                                  label="执行参数">
                                    </v-text-field>
                                </v-tab-item>
                            </v-tabs>
                        </template>
                        <v-btn :disabled="!valid || submitting" color="primary" @click="submit">保存</v-btn>
                    </v-form>
                </v-container>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./config.js"></script>