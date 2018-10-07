<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>比赛列表</h2>
                <v-spacer></v-spacer>
                <v-tooltip v-if="user && user.privilege >= 1 && user.privilege <= 2" bottom>
                    <v-btn icon slot="activator" @click="addContest">
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
                        <td v-if="user && user.privilege >= 1 && user.privilege <= 2">
                            <v-layout row>
                                <v-tooltip bottom>
                                    <v-btn icon slot="activator" @click="editContest(props.item.id)">
                                        <v-icon color="primary">edit</v-icon>
                                    </v-btn>
                                    <span>编辑</span>
                                </v-tooltip>
                                <v-tooltip bottom>
                                    <v-btn icon slot="activator" @click="deleteContest(props.item.id)">
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
            <v-card-actions>
                <v-layout justify-center align-center>
                    <v-pagination circle v-model="page" :length="pageCount" :total-visible="7"></v-pagination>
                </v-layout>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./contest.js"></script>