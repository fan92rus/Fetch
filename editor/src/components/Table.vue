<template>
  <div class="table">
    <h2>{{table.Name}}</h2>
    <table>
      <thead>
        <tr>
          <th v-for="item in properties" :key="item">{{ item }}</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row,index) in table.Properties" :key="index">
          <td v-for="prop in properties" :key="prop">{{ row[prop] }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<script>
export default {
  props: {
    table: {
      type: Object,
      required: true,
    },
  },
  methods: {
    getProp(item, prop) {
      console.log("item " + JSON.stringify(item));
      console.log("prop " + prop);
      console.log(item[prop]);
      return item[prop];
    },
  },
  computed: {
    properties() {
      let names = [];
      for (const row of this.table.Properties) {
        for (const key in row) {
          if (!names.includes(key)) {
            names.push(key);
          }
        }
      }
      console.log(names);
      return names;
    },
  },
};
</script>
<style lang="scss">
table {
  font-family: "Lucida Sans Unicode", "Lucida Grande", Sans-Serif;
  border-collapse: collapse;
  color: #686461;
}
caption {
  padding: 10px;
  color: white;
  background: #8fd4c1;
  font-size: 18px;
  text-align: left;
  font-weight: bold;
}
th {
  border-bottom: 3px solid #b9b29f;
  padding: 10px;
  text-align: left;
}
td {
  padding: 10px;
}
tr:nth-child(odd) {
  background: white;
}
tr:nth-child(even) {
  background: #e8e6d1;
}
</style>