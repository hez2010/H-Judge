<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>比赛列表</h2>
                <v-spacer></v-spacer>
                <v-tooltip v-if="user && user.privilege >= 1 && user.privilege <= 3" bottom>
                    <v-btn icon slot="activator">
                        <v-icon color="primary">add</v-icon>
                    </v-btn>
                    <span>添加</span>
                </v-tooltip>
            </v-card-title>
            <v-card-text>
                <v-data-table :headers="headers"
                              :items="contests"
                              :custom-sort="sortRules"
                              hide-actions>
                    <template slot="items" slot-scope="props">
                        <td>{{ props.item.id }} <span v-if="props.item.hidden">(隐藏)</span></td>
                        <td><a @click="toDetails(props.item.id)">{{ props.item.name }}</a></td>
                        <td>{{ props.item.startTime }}</td>
                        <td>{{ props.item.endTime }}</td>
                        <td>{{ props.item.problemCount }}</td>
                        <td>{{ props.item.status }}</td>
                        <td v-if="user && user.privilege >= 1 && user.privilege <= 3">
                            <v-layout row>
                                <v-tooltip bottom>
                                    <v-btn icon slot="activator">
                                        <v-icon color="primary">edit</v-icon>
                                    </v-btn>
                                    <span>编辑</span>
                                </v-tooltip>
                                <v-tooltip bottom>
                                    <v-btn icon slot="activator">
                                        <v-icon color="red">delete</v-icon>
                                    </v-btn>
                                    <span>删除</span>
                                </v-tooltip>
                            </v-layout>
                        </td>
                    </template>
                    <template slot="no-data">
                        <p v-if="loading">正在加载...</p>
                        <p v-else>没有数据 :(</p>
                    </template>
                </v-data-table>
            </v-card-text>
            <v-card-action>
                <div class="text-xs-center">
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="7"></v-pagination>
                </div>
            </v-card-action>
        </v-card>
    </v-container>
</template>

<script src="./contest.js"></script>