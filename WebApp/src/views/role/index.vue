<template>
  <div class="app-container">
    <!--工具栏-->
    <div class="head-container">
      <!-- 搜索 -->
      <el-input
        v-model="listQuery.Filter"
        clearable
        size="small"
        placeholder="搜索..."
        style="width: 200px"
        class="filter-item"
        @keyup.enter.native="handleFilter"
      />
      <el-button
        class="filter-item"
        size="mini"
        type="success"
        icon="el-icon-search"
        @click="handleFilter"
        >搜索</el-button
      >
      <!-- <el-button
        class="filter-item"
        size="mini"
        type="warning"
        icon="el-icon-refresh-left"
        @click="resetQuery"
      >重置</el-button> -->
      <div class="opts">
        <el-button
          class="filter-item"
          size="mini"
          type="primary"
          icon="el-icon-plus"
          @click="handleCreate"
          v-permission="['AbpIdentity.Roles.Create']"
          >新增</el-button
        >
        <!-- <el-button
          class="filter-item"
          size="mini"
          type="success"
          icon="el-icon-edit"
          v-permission="['AbpIdentity.Roles.Update']"
          @click="handleUpdate()"
          >修改</el-button
        >
        <el-button
          slot="reference"
          class="filter-item"
          type="danger"
          icon="el-icon-delete"
          size="mini"
          v-permission="['AbpIdentity.Roles.Delete']"
          @click="handleDelete()"
          >删除</el-button
        > -->
      </div>
    </div>
    <!--表单渲染-->
    <el-dialog
      :visible.sync="dialogFormVisible"
      :close-on-click-modal="false"
      :title="formTitle"
      width="500px"
    >
      <el-form
        ref="form"
        :inline="true"
        :model="form"
        :rules="rules"
        size="small"
        label-width="66px"
      >
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" style="width: 380px" />
        </el-form-item>
        <el-form-item label="属性">
          <el-checkbox v-model="form.isDefault">默认</el-checkbox>
          <el-checkbox v-model="form.isPublic">公共</el-checkbox>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button size="small" type="text" @click="cancel">取消</el-button>
        <el-button
          size="small"
          v-loading="formLoading"
          type="primary"
          @click="save"
          >确认</el-button
        >
      </div>
    </el-dialog>

    <el-row :gutter="15">
      <!--角色管理-->
      <el-col :md="16" style="margin-bottom: 10px">
        <el-card class="box-card" shadow="never">
          <div slot="header" class="clearfix" style="height: 20px">
            <span class="role-span">角色列表</span>
          </div>
          <!--表格渲染-->
          <el-table
            ref="multipleTable"
            v-loading="listLoading"
            :data="list"
            size="small"
            style="width: 100%"
            @sort-change="sortChange"
            @selection-change="handleSelectionChange"
            @row-click="handleRowClick"
          >
            <!-- <el-table-column type="selection" width="44px"></el-table-column> -->
            <el-table-column
              label="角色名"
              prop="name"
              sortable="custom"
              align="center"
            >
              <template slot-scope="{ row }">
                <span class="link-type" @click="handleUpdate(row)">{{
                  row.name
                }}</span>
              </template>
            </el-table-column>
            <el-table-column
              label="默认"
              prop="isDefault"
              align="center"
              width="100px"
            >
              <template slot-scope="scope">
                <span>{{ scope.row.isDefault | displayStatus }}</span>
              </template>
            </el-table-column>
            <el-table-column
              label="公共"
              prop="isPublic"
              align="center"
              width="100px"
            >
              <template slot-scope="scope">
                <span>{{ scope.row.isPublic | displayStatus }}</span>
              </template>
            </el-table-column>
            <el-table-column label="描述" prop="description" align="center">
              <template slot-scope="scope">
                <span>{{ scope.row.description }}</span>
              </template>
            </el-table-column>
            <el-table-column label="操作" align="center" width="200">
              <template slot-scope="{ row }">
                <el-button
                  type="primary"
                  size="mini"
                  @click="handleUpdate(row)"
                  v-permission="['AbpIdentity.Roles.Update']"
                  icon="el-icon-edit"
                />
                <el-button
                  type="danger"
                  size="mini"
                  @click="handleDelete(row)"
                  :disabled="row.name === 'admin'"
                  v-permission="['AbpIdentity.Roles.Delete']"
                  icon="el-icon-delete"
                />
                <el-button
                  type="warning"
                  size="mini"
                  @click="handleCopy(row)"
                  :disabled="row.name === 'admin'"
                  v-permission="['AbpIdentity.Roles.Delete']"
                  icon="el-icon-document-copy"
                />
              </template>
            </el-table-column>
          </el-table>
          <pagination
            v-show="totalCount > 0"
            :total="totalCount"
            :page.sync="page"
            :limit.sync="listQuery.MaxResultCount"
            @pagination="getList"
          />
        </el-card>
      </el-col>

      <el-col :md="8">
        <el-card class="box-card" shadow="never">
          <div slot="header" style="height: 20px; line-height: 20px">
            <el-tooltip
              class="item"
              effect="dark"
              content="选择指定角色的菜单权限"
              placement="top"
            >
              <span>菜单权限</span>
            </el-tooltip>
            <span style="float: right; margin-top: -8px">
              <el-checkbox
                v-model="checked"
                @change="checkedAll"
                :disabled="multipleSelection.length != 1"
              >
                全选</el-checkbox
              >
              <el-button
                v-permission="['AbpIdentity.Roles.ManagePermissions']"
                :loading="savePerLoading"
                :disabled="multipleSelection.length != 1"
                icon="el-icon-check"
                type="text"
                @click="savePer"
                >保存</el-button
              >
            </span>
          </div>
          <!-- <el-tree
            ref="tree"
            v-loading="treeLoading"
            :default-checked-keys="checkedPermission"
            :check-strictly="true"
            :data="permissionsData"
            :props="defaultProps"
            show-checkbox
            node-key="id"
            @check="checkNode"
            class="permission-tree"
          /> -->
          <el-tree
            ref="tree"
            v-loading="treeLoading"
            :check-strictly="true"
            :data="menus"
            show-checkbox
            node-key="id"
            @check="checkNode"
            class="permission-tree"
          />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script>
import { isvalidPhone } from "@/utils/validate";
import Pagination from "@/components/Pagination";
import permission from "@/directive/permission/index.js";

export default {
  name: "Role",
  components: { Pagination },
  directives: { permission },
  filters: {
    displayStatus(status) {
      const statusMap = {
        true: "是",
        false: "否",
      };
      return statusMap[status];
    },
  },
  data() {
    return {
      rules: {
        name: [{ required: true, message: "请输入角色名", trigger: "blur" }],
      },
      form: {},
      list: null,
      menuData: [],
      menus: [],
      totalCount: 0,
      listLoading: true,
      formLoading: false,
      treeLoading: false,
      savePerLoading: false,
      listQuery: {
        Filter: "",
        Sorting: "",
        SkipCount: 0,
        MaxResultCount: 30,
      },
      page: 1,
      dialogFormVisible: false,
      multipleSelection: [],
      formTitle: "",
      isEdit: false,
      checked: false,
    };
  },
  created() {
    this.getList();
    this.getMenuData();
  },
  methods: {
    // 拷贝角色
    handleCopy(row) {
      let roleslist = [];
      let obj = {
        name: row.name + "_copy",
        isDefault: row.isDefault,
      };
      this.$axios.gets("/api/base/role-menus/" + row.id).then((response) => {
        roleslist = response.items;
      });
      this.$axios
        .posts("/api/identity/roles", obj)
        .then((response) => {
          this.$message({
            message: "拷贝成功",
            type: "success",
          });
          this.getList();
          let roleobj = {
            roleId: response.id,
            menuIds: roleslist,
          };
          this.$axios
            .posts("/api/base/role-menus/update", roleobj)
            .then(() => {
              this.$notify({
                title: "成功",
                message: "更新成功",
                type: "success",
                duration: 1000,
              });
            })
            .catch(() => {
              this.$notify({
                title: "失败",
                message: "复制",
                type: "success",
                duration: 1000,
              });
            });
        })
        .catch(() => {});
    },
    getMenuData() {
      this.treeLoading = true;
      this.$axios.gets("/api/base/role-menus/tree").then((response) => {
        this.menuData = response.items;
        this.menus = response.items.filter((_) => _.pid == null);
        this.setChildren(this.menus, response.items);
        this.treeLoading = false;
      });
    },
    getList() {
      this.listLoading = true;
      this.listQuery.SkipCount =
        (this.page - 1) * this.listQuery.MaxResultCount;
      this.$axios
        .gets("/api/identity/roles", this.listQuery)
        .then((response) => {
          this.list = response.items;
          this.totalCount = response.totalCount;
          this.listLoading = false;
        });
    },
    fetchData(id) {
      this.$axios.gets("/api/identity/roles/" + id).then((response) => {
        this.form = response;
      });
    },
    handleFilter() {
      this.page = 1;
      this.getList();
    },
    resetQuery() {},
    save() {
      this.$refs.form.validate((valid) => {
        if (valid) {
          this.formLoading = true;
          if (this.isEdit) {
            this.$axios
              .puts("/api/identity/roles/" + this.form.id, this.form)
              .then((response) => {
                this.formLoading = false;
                this.$notify({
                  title: "成功",
                  message: "更新成功",
                  type: "success",
                  duration: 1000,
                });
                this.dialogFormVisible = false;
                this.getList();
              })
              .catch(() => {
                this.formLoading = false;
              });
          } else {
            this.$axios
              .posts("/api/identity/roles", this.form)
              .then((response) => {
                this.formLoading = false;
                this.$notify({
                  title: "成功",
                  message: "新增成功",
                  type: "success",
                  duration: 1000,
                });
                this.dialogFormVisible = false;
                this.getList();
              })
              .catch(() => {
                this.formLoading = false;
              });
          }
        }
      });
    },
    savePer() {
      this.savePerLoading = true;
      let params = {};
      let checkedKeys = this.$refs.tree.getCheckedKeys();
      params.permissions = [];
      this.menuData.forEach((element) => {
        if (element.permission) {
          let perm = {};
          perm.name = element.permission;
          if (checkedKeys.indexOf(element.id) > -1) {
            perm.isGranted = true;
          } else {
            perm.isGranted = false;
          }
          params.permissions.push(perm);
        }
      });
      this.$axios
        .puts(
          "/api/permission-management/permissions?providerName=R&providerKey=" +
            this.multipleSelection[0].name,
          params
        )
        .then((ress) => {
          console.log(55555555555, ress);
          this.$axios
            .posts("/api/base/role-menus/update", {
              roleId: this.multipleSelection[0].id,
              menuIds: checkedKeys,
            })
            .then(() => {
              this.$notify({
                title: "成功",
                message: "更新成功",
                type: "success",
                duration: 1000,
              });
              this.savePerLoading = false;
            })
            .catch(() => {
              this.savePerLoading = false;
            });
        })
        .catch(() => {
          this.savePerLoading = false;
        });
      // 新增接口
      this.$axios
        .puts(
          "/api/base/role-menus/AddPermission?providerName=R&providerKey=" +
            this.multipleSelection[0].name,
          params
        )
        .then((ress) => {})
        .catch(() => {
          this.savePerLoading = false;
        });
    },
    handleCreate() {
      this.formTitle = "新增角色";
      this.isEdit = false;
      this.form = {};
      this.dialogFormVisible = true;
    },
    handleDelete(row) {
      if (row) {
        this.$confirm("是否删除" + row.name + "?", "提示", {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning",
        })
          .then(() => {
            this.$axios
              .deletes("/api/identity/roles/" + row.id)
              .then((response) => {
                const index = this.list.indexOf(row);
                this.list.splice(index, 1);
                this.$notify({
                  title: "成功",
                  message: "删除成功",
                  type: "success",
                  duration: 1000,
                });
              });
          })
          .catch(() => {
            this.$message({
              type: "info",
              message: "已取消删除",
            });
          });
      } else {
        this.$alert("暂时不支持角色批量删除", "提示", {
          confirmButtonText: "确定",
          callback: (action) => {
            //
          },
        });
      }
    },
    handleUpdate(row) {
      this.formTitle = "修改角色";
      this.isEdit = true;

      if (row) {
        this.fetchData(row.id);
        this.dialogFormVisible = true;
      } else {
        if (this.multipleSelection.length != 1) {
          this.$message({
            message: "编辑必须选择单行",
            type: "warning",
          });
          return;
        } else {
          this.fetchData(this.multipleSelection[0].id);
          this.dialogFormVisible = true;
        }
      }
    },
    sortChange(data) {
      const { prop, order } = data;
      if (!prop || !order) {
        this.handleFilter();
        return;
      }
      this.listQuery.Sorting = prop + " " + order;
      this.handleFilter();
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    handleRowClick(row, column, event) {
      if (
        this.multipleSelection.length == 1 &&
        this.multipleSelection[0].id == row.id
      ) {
        return;
      }
      this.treeLoading = true;
      this.$refs.multipleTable.clearSelection();
      this.$refs.multipleTable.toggleRowSelection(row);
      this.$axios.gets("/api/base/role-menus/" + row.id).then((response) => {
        this.$refs.tree.setCheckedKeys(response.items);
        this.treeLoading = false;
      });
    },
    setChildren(roots, items) {
      roots.forEach((element) => {
        items.forEach((item) => {
          if (item.pid == element.id) {
            if (!element.children) element.children = [];
            element.children.push(item);
          }
        });
        if (element.children) {
          this.setChildren(element.children, items);
        }
      });
    },
    checkedAll() {
      if (this.checked) {
        //全选
        this.$refs.tree.setCheckedNodes(this.menuData);
      } else {
        //取消选中
        this.$refs.tree.setCheckedKeys([]);
      }
    },
    checkNode(data, state) {
      if (!data.pid) {
        // if (state.checkedKeys.indexOf(data.id) === -1) {
        //   data.children.forEach(element => {
        //     this.$refs.tree.setChecked(element.id, false);
        //   });
        // }
      } else {
        if (state.checkedKeys.indexOf(data.id) > -1) {
          this.$refs.tree.setChecked(data.pid, true);
        }
      }
    },
    cancel() {
      this.dialogFormVisible = false;
      this.$refs.form.clearValidate();
    },
  },
};
</script>

<style rel="stylesheet/scss" lang="scss" scoped>
.opts {
  padding: 6px 0;
  display: -webkit-flex;
  display: flex;
  align-items: center;
}
</style>
